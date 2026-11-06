namespace Alohomora.Core.Common.Helpers;

public static class ReturnUrlHelper
{
    public static bool IsValidLocalUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        // Must start with '/' but NOT '//' or '/\' to prevent protocol-relative or absolute redirects
        if (returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
        {
            // Optional: Ensure it doesn't contain a URI scheme or host definition
            // Uri.IsWellFormedUriString handles relative paths if checked carefully,
            // but for a strict local relative path check, Uri.TryCreate is safer:

            // If it can be parsed as an absolute URI, it's not a pure relative local path.
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
