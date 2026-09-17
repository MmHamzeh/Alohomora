using Alohomora.Core.Common.Helpers;
using Alohomora.PayamResan.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Alohomora.PayamResan.Services;

public class PayamResanV3ServiceTemplate(IHttpClientFactory httpClientFactory, string apiKey, long sender)
    
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PayamResanV3");


    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد Get (Send )
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="message"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // 1. Build Query Parameters
        var queryParams = new Dictionary<string, string?>
        {
            ["ApiKey"] = apiKey,
            ["Text"] = message,
            ["Sender"] = sender.ToString(),
            ["Recipients"] = phoneNumber
        };
    
        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}"));
    
        var url = $"Send?{queryString}";    
        // 2. Execute GET Request
        using var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
    
        // 3. Read Content and Deserialize
        var contentStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        var finalResult = await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SendSMSOutput>>>(contentStream);
    
        return finalResult?.Success ?? false;
    }


    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد post (SendBulk )
    /// </summary>
    /// <param name="phoneNumberList"></param>
    /// <param name="message"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(List<string> phoneNumberList, string message,
        CancellationToken ct = default)
    {
        var dto = new SendBulkSMSDto
        {
            ApiKey = apiKey,
            Sender = sender,
            Text = message,
            Recipients = phoneNumberList.Select(phoneNumber => new SendBulkRecipient
            {
                Destination = long.Parse(phoneNumber),
                UserTraceId = 0
            }).ToArray()
        };
    
        // PostAsJsonAsync serializes dto directly to the request stream
        using var response = await _httpClient.PostAsJsonAsync("SendBulk", dto, ct);
        response.EnsureSuccessStatusCode();
    
        // Stream directly into your deserializer to avoid LOH / string allocations
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        var finalResult = await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SendSMSOutput>>>(responseStream);
    
        return finalResult?.Success ?? false;
    }


    /// <summary>
    /// متد ارسال یک متن به چند شماره با متد post (SendMultiple )
    /// </summary>
    /// <param name="phoneNumberMessageList"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(List<Tuple<string, string>> phoneNumberMessageList, CancellationToken ct = default)
    {
        var dto = new SendMultipleSMSDto
        {
            ApiKey = apiKey,
            Recipients = phoneNumberMessageList.Select(item => new SendMultipleRecipient
            {
                Destination = long.Parse(item.Item1),
                Text = item.Item2,
                Sender = sender,
                UserTraceId = 0
            }).ToArray()
        };

        var json = JsonHelper.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8);

        using var response = await _httpClient.PostAsync("SendMultiple", content, ct);
        response.EnsureSuccessStatusCode();
    
        // Stream-based deserialization to avoid intermediate string allocations
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        var finalResult = await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SendSMSOutput>>>(responseStream);
    
        return finalResult?.Success ?? false;
    }




    #region Token

    /// <summary>
    /// متد ارسال به یک شماره با استفاده از الگوی تعریف شده (SendTokenSingle)
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="parameters"></param>
    /// <param name="templateKey"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SendTokenOutput>>> SendTokenSingle(string templateKey, string destination, string[]? parameters = null, CancellationToken ct = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["ApiKey"] = apiKey,
            ["TemplateKey"] = templateKey,
            ["Destination"] = destination
        };
    
        // Dynamically map parameters to p1, p2, p3, ..., pn
        if (parameters is { Length: > 0 })
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!string.IsNullOrEmpty(parameters[i]))
                {
                    queryParams[$"p{i + 1}"] = parameters[i];
                }
            }
        }
    
        var requestUri = QueryHelpers.AddQueryString("SendTokenSingle", queryParams);
    
        using var response = await _httpClient.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
    
        var contentStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        var finalResult = await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SendTokenOutput>>>(contentStream);
    
        return finalResult!;
    }

    /// <summary>
    /// متد ارسال به چند شماره با استفاده از الگوی تعریف شده (SendTokenMulti)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SendTokenOutput>>> SendMultiPleTokenAsync(SendMultipleTokenDto input, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("SendTokenMulti", input, ct);
        response.EnsureSuccessStatusCode();
    
        var contentStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        var finalResult = await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SendTokenOutput>>>(contentStream);
    
        return finalResult!;

    }

    /// <summary>
    /// متد دریافت لیست الگوهای تعریف شده در پنل (TokenList)
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<TokenListOutput>>?> TokenList(BaseUser user, CancellationToken ct)
    {
        var json = JsonHelper.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8);

        var response = await _httpClient.PostAsync("TokenList", content, ct);
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);

        return await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<TokenListOutput>>>(responseStream);
    }

    #endregion

    #region StatusEnm

    /// <summary>
    /// متد دریافت وضعیت پیام های ارسالی (StatusById)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SMSStatusOutput>>?> StatusById(GetStatusByIdDto input, CancellationToken ct = default)
    {
        var json = JsonHelper.Serialize(input);
        var content = new StringContent(json, Encoding.UTF8);

        var response = await _httpClient.PostAsync("StatusById", content, ct);
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);

        return await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SMSStatusOutput>>>(responseStream);
    }

    /// <summary>
    /// متد دریافت وضعیت پیام های ارسالی (StatusByTraceId)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SMSStatusOutput>>?> StatusByUserTraceId(GetStatusByUserTraceIdsDto input, CancellationToken ct = default)
    {
        var json = JsonHelper.Serialize(input);
        var content = new StringContent(json, Encoding.UTF8);

        var response = await _httpClient.PostAsync("StatusByUserTraceId", content, ct);
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);

        return await JsonHelper.DeserializeAsync<SMSOutputGenericModel<List<SMSStatusOutput>>>(responseStream);

    }

    #endregion

    #region Account

    /// <summary>
    /// متد دریافت مقدار اعتبار و لیست خطوط ارسال کننده فعال در پنل (AccountInfo)
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<SMSAcountIfoOutput>?> AccountInfo(BaseUser user, CancellationToken ct = default)
    {
        var json = JsonHelper.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8);

        var response = await _httpClient.PostAsync("AccountInfo", content, ct);
        var responseStream = await response.Content.ReadAsStreamAsync(CancellationToken.None);

        return JsonSerializer.Deserialize<SMSOutputGenericModel<SMSAcountIfoOutput>>(responseStream);
    }

    #endregion

    #region Document

    //    لیست کد خطا های دریافتی از سرویس
    //{کد 1} => کلید اتصال (ApiKey) نادرست است
    //{کد 2} => ارسال از وبسرویس برای این کاربر فعال نشده است.
    //{کد 3} => کاربر فعال نیست
    //{کد 4} => ارسال از طریق Api موقتا غیر فعال است.
    //{کد 5} => ارسال از طریق این متد موقتا غیر فعال است.
    //{کد 6} => تلفن همراه ثبت شده روی این اکانت تائید نشده است
    //{کد 7} => ایمیل ثبت شده روی این اکانت تائید نشده است
    //{کد 8} => اطلاعات هویتی کاربر به صورت کامل درج نشده است
    //{کد 9} => مقادیر ورودی نادرست است.
    //{کد 10} => آی پی درخواست دهنده در لیست آی پی های معتبر نیست
    //{کد 11} => فرمت ورودی ApiKey نادرست است
    //{کد 12} => شماره ارسال کننده نادرست است
    //{کد 13} => شماره دریافت کننده باید با 9 یا 989 شروع شود
    //{کد 14} => شما به این متد دسترسی ندارید
    //{کد 15} => سقف ارسال روزانه به پایان رسید


    //وضعیت های الگوهای تعریف شده
    //{وضعیت 1} => در انتظار تایید
    //{وضعیت 2} => تایید شده
    //{وضعیت 3} => رد شده


    //لیست وضعیت های پیامک ارسالی
    //{وضعیت 0} => در صف سامانه
    //{وضعیت 1} => ارسال شده، بدون وضعیت
    //{وضعیت 2} => ارسال شده، در صف اپراتور
    //{وضعیت 3} => درانتظار تحویل گوشی
    //{وضعیت 4} => تحویل به گوشی
    //{وضعیت 5} => نرسیده به گوشی
    //{وضعیت 6} => خطا در ارسال
    //{وضعیت 7} => گیرنده مسدود
    //{وضعیت 8} => شناسه پیدا نشد
    //{وضعیت 9} => منقضی
    //{وضعیت 10} => نامشخص
    //{وضعیت 11} => دریافت کننده نامعتبر
    //{وضعیت 12} => گیرنده در لیست سیاه اکانت شماست
    //{وضعیت 13} => گیرنده نامعتبر
    //{وضعیت 14} => متن بلاک
    //{وضعیت 15} => لغو دریافت توسط گیرنده
    //{وضعیت 16} => بلاک شده
    //{وضعیت 17} => محدودیت روزانه

    #endregion
}
