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

    public static bool CreateUserIfNotExists { get; set; } = true;

    public static bool SendLoginSmsNotification { get; set; } = false;
    public static bool SendLoginEmailNotification { get; set; } = false;

    public static bool SendPasswordChangedSmsNotification { get; set; } = false;
    public static bool SendPasswordChangedPasswordNotification { get; set; } = false;

    public static bool SendInvalidLoginAttemptSmsNotification { get; set; } = false;
    public static bool SendInvalidLoginAttemptEmailNotification { get; set; } = false;

    public static bool SendAccountLockedSmsNotification { get; set; } = false;
    public static bool SendAccountLockedEmailNotification { get; set; } = false;

    public static int AccessTokenExpirationMinutes = 10;
    public static int OtpExpirationMinutes = 5;
}
