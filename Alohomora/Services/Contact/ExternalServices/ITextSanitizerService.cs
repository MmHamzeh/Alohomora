namespace Alohomora.Core.Services.Contact.ExternalServices;


public interface ITextSanitizerService
{
    string Sanitize(string text);
}
