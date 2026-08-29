namespace Alohomora.Core.Common.Configs;

public static class ApplicationSetting
{
#if DEBUG
    public const bool IsDebugMode = true;
#else
    //public const bool IsDebugMode = false;
    public const bool IsDebugMode = true;
#endif



    //TODO: Get these from appsettings.json
    public const string ApiDomainName = "api.domain.ir";
    public const string AuthDomainName = "auth.domain.ir";
    public const string ApplicationName = "ApplicationName";

    public static string RsaKeysDirectory = "rsa-keys";

    public const int AccessTokenExpirationMinutes = 10;
}
