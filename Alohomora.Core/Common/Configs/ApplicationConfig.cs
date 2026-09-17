namespace Alohomora.Core.Common.Configs;

public static class ApplicationConfig
{

    public static string ConfirmPhoneNumberUrl { get; set; }

    //"/Auth/ConfirmPhoneNumber?returnUrl={returnUrl}"
    public static string ConfirmPhoneNumberUrlWithReturnUrl { get; set; }
}
