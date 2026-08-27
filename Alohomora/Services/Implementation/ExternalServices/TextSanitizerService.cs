using Alohomora.Core.Services.Contact.ExternalServices;
using Ganss.Xss;

namespace Alohomora.Core.Services.Implementation.ExternalServices;

public class TextSanitizerService : ITextSanitizerService
{
    private readonly HtmlSanitizer _htmlSanitizer;

    public TextSanitizerService(HtmlSanitizer htmlSanitizer)
    {
        _htmlSanitizer = htmlSanitizer;
    }

    public string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }
        // Sanitize the text to remove any potentially harmful HTML or scripts
        return _htmlSanitizer.Sanitize(text);
    }
}