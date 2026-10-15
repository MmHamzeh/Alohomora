namespace Alohomora.Core.Common.Configs;

public static class ApplicationConfig
{

    //TODO: Get these from appsettings.json
    public static string ApplicationName { get; set; } = "ApplicationName";
    public static string ApplicationDomain { get; set; } = "domain.ir";
    public static string ApiDomain { get; set; } = "api.domain.ir";
    public static string AuthDomain { get; set; } = "auth.domain.ir";


    public static string ConfirmPhoneNumberUrl { get; set; } = "/Login-Confirm";
    public static string ConfirmPhoneNumberUrlWithReturnUrl { get; set; } = "/Login-Confirm?returnUrl={0}";


}
