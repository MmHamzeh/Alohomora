using System.Diagnostics;

namespace Alohomora.Core.Services.Contact.ExternalServices;

internal class NoOpEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // Default no-op implementation: log to diagnostic output and return success
        Debug.WriteLine($"[NoOpEmailService] SendEmailAsync to={to} subject={subject} body={body}");
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAsync(List<string> to, string subject, string body, CancellationToken ct = default)
    {
        Debug.WriteLine($"[NoOpEmailService] SendEmailAsync to={string.Join(',', to)} subject={subject} body={body}");
        return Task.FromResult(true);
    }
}
