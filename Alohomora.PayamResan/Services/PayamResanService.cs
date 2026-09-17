using Alohomora.Core.Common.Helpers;
using Alohomora.Core.Services.Contact.ExternalServices;
using Alohomora.PayamResan.Models;
using System.Net.Http.Json;
using System.Text;

namespace Alohomora.PayamResan.Services;

public class PayamResanService(IHttpClientFactory httpClientFactory, string apiKey, long sender)
    : ISmsService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PayamResanV3");

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
}