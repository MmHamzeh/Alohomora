namespace Alohomora.Services.Contact.ExternalServices;


public interface ITextSanitizerService
{
    string Sanitize(string text);
}
