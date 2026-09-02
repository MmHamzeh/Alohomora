namespace Alohomora.Core.Services.Contact.ExternalServices;

public interface IEmailService
{
    /// <summary>
    /// Send a simple email. Returns true when the email provider accepted the message for delivery.
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);

    /// <summary>
    /// Send an email to multiple recipients. Returns true when the provider accepted the message for delivery.
    /// </summary>
    Task<bool> SendEmailAsync(List<string> to, string subject, string body, CancellationToken ct = default);
}
