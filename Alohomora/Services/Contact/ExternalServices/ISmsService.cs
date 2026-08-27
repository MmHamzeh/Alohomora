namespace Alohomora.Core.Services.Contact.ExternalServices;

public interface ISmsService
{
    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد 
    /// </summary>
    Task<bool> SendMessageAsync(string phoneNumber,
                                string message,
                                CancellationToken ct = default);

    /// <summary>
    /// متد ارسال یک متن به یک یا چند شماره با متد 
    /// </summary>
    Task<bool> SendMessageAsync(List<string> phoneNumber,
                                string message,
                                CancellationToken ct = default);

    /// <summary>
    /// متد ارسال یک متن به چند شماره با متد 
    /// </summary>
    Task<bool> SendMessageAsync(List<Tuple<string, string>> phoneNumberMessageList,
                                CancellationToken ct = default);
}
