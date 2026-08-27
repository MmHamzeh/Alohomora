using Alohomora.Core.Common.Helpers;
using Alohomora.Core.Domain.Models.DtoModels.ExternalModels;
using Alohomora.Core.Domain.Models.ViewModels.ExternalModels;
using Alohomora.Core.Services.Contact.ExternalServices;

namespace Alohomora.Core.Services.Implementation.ExternalServices;

public class PayamResanService : ISmsService
{
    private static readonly RestClient _client = new("http://api.sms-webservice.com/api/V3/");
    private const string ApiKey = ""; // Replace with your actual API key
    private const long Sender = 0L; // Replace with your actual sender Number

    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد Get (Send )
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var request = new RestRequest($"Send").AddQueryParameter("ApiKey", ApiKey, encode: true);
        request.AddQueryParameter("Text", message, encode: true);
        request.AddQueryParameter("Sender", Sender);
        request.AddQueryParameter("Recipients", phoneNumber);

        var response = await _client.ExecuteGetAsync(request, ct);
        var finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SendSMSOutput>>>(response.Content);
        return finalresult.Success;
    }


    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد post (SendBulk )
    /// </summary>
    /// <param name="phoneNumberList"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(List<string> phoneNumberList, string message, CancellationToken ct = default)
    {
        SendBulkSMSDto dto = new()
        {
            ApiKey = ApiKey,
            Sender = Sender,
            Text = message,

            Recipients = phoneNumberList.Select(phoneNumber => new SendBulkRecipient
            {
                Destination = long.Parse(phoneNumber),
                UserTraceId = 0 // Assuming UserTraceId is not required for bulk messages
            }).ToArray()
        };

        var request = new RestRequest($"SendBulk").AddJsonBody(dto);
        var response = await _client.ExecutePostAsync(request, ct);
        SMSOutputGenericModel<List<SendSMSOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SendSMSOutput>>>(response.Content);
        return finalresult.Success;
    }


    /// <summary>
    /// متد ارسال یک متن به چند شماره با متد post (SendMultiple )
    /// </summary>
    /// <param name="phoneNumberMessageList"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SendMessageAsync(List<Tuple<string, string>> phoneNumberMessageList, CancellationToken ct = default)
    {
        SendMultipleSMSDto dto = new()
        {
            ApiKey = ApiKey,
            Recipients = phoneNumberMessageList.Select(phoneNumberMessage => new SendMultipleRecipient
            {
                Destination = long.Parse(phoneNumberMessage.Item1),
                Text = phoneNumberMessage.Item2,
                Sender = Sender,
                UserTraceId = 0 // Assuming UserTraceId is not required for bulk messages
            }).ToArray()
        };

        var request = new RestRequest($"SendMultiple").AddJsonBody(dto);
        var response = await _client.ExecutePostAsync(request, ct);
        var finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SendSMSOutput>>>(response.Content);
        return finalresult.Success;
    }




    #region Token

    /// <summary>
    /// متد ارسال به یک شماره با استفاده از الگوی تعریف شده (SendTokenSingle)
    /// </summary>
    /// <param name="ApiKey"></param>
    /// <param name="TemplateKey"></param>
    /// <param name="Destination"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SendTokenOutput>>> SendTokenSingle(string ApiKey, string TemplateKey, long Destination, string p1, string p2, string p3)
    {

        var request = new RestRequest($"SendTokenSingle").AddQueryParameter("ApiKey", ApiKey, true);
        request.AddQueryParameter("TemplateKey", TemplateKey);
        request.AddQueryParameter("Destination", Destination);
        if (p1.Length > 0) request.AddQueryParameter("p1", p1);
        if (p1.Length > 0) request.AddQueryParameter("p2", p2);
        if (p1.Length > 0) request.AddQueryParameter("p3", p3);

        var response = await _client.ExecuteGetAsync(request);
        SMSOutputGenericModel<List<SendTokenOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SendTokenOutput>>>(response.Content);
        return finalresult;

    }

    /// <summary>
    /// متد ارسال به چند شماره با استفاده از الگوی تعریف شده (SendTokenMulti)
    /// </summary>
    /// <param name="Input"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SendTokenOutput>>> SendMultiPleTokenAsync(SendMultipleTokenDto Input)
    {
        var request = new RestRequest($"SendTokenMulti").AddJsonBody(Input);
        var response = await _client.ExecutePostAsync(request);
        SMSOutputGenericModel<List<SendTokenOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SendTokenOutput>>>(response.Content);
        return finalresult;

    }

    /// <summary>
    /// متد دریافت لیست الگوهای تعریف شده در پنل (TokenList)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<TokenListOutput>>> TokenList(BaseUser user)
    {
        var request = new RestRequest($"TokenList").AddJsonBody(user);
        var response = await _client.ExecutePostAsync(request);
        SMSOutputGenericModel<List<TokenListOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<TokenListOutput>>>(response.Content);
        return finalresult;

    }

    #endregion

    #region StatusEnm

    /// <summary>
    /// متد دریافت وضعیت پیام های ارسالی (StatusById)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SMSStatusOutput>>> StatusById(GetStatusByIdDto input)
    {
        var request = new RestRequest($"StatusById").AddJsonBody(input);
        var response = await _client.ExecutePostAsync(request);
        SMSOutputGenericModel<List<SMSStatusOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SMSStatusOutput>>>(response.Content);
        return finalresult;

    }

    /// <summary>
    /// متد دریافت وضعیت پیام های ارسالی (StatusByTraceId)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<List<SMSStatusOutput>>> StatusByUserTraceId(GetStatusByUserTraceIdsDto input)
    {
        var request = new RestRequest($"StatusByUserTraceId").AddJsonBody(input);
        var response = await _client.ExecutePostAsync(request);
        SMSOutputGenericModel<List<SMSStatusOutput>> finalresult = JsonHelper.Deserialize<SMSOutputGenericModel<List<SMSStatusOutput>>>(response.Content);
        return finalresult;

    }

    #endregion

    #region Account

    /// <summary>
    /// متد دریافت مقدار اعتبار و لیست خطوط ارسال کننده فعال در پنل (AccountInfo)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    internal async Task<SMSOutputGenericModel<SMSAcountIfoOutput>> AccountInfo(BaseUser user)
    {
        var request = new RestRequest($"AccountInfo").AddJsonBody(user);
        var response = await _client.ExecutePostAsync(request);
        SMSOutputGenericModel<SMSAcountIfoOutput> finalresult = JsonSerializer.Deserialize<SMSOutputGenericModel<SMSAcountIfoOutput>>(response.Content);
        return finalresult;

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
