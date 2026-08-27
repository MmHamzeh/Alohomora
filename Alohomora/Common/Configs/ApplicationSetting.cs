namespace Alohomora.Common.Configs;

internal static class ApplicationSetting
{
#if DEBUG
    internal const bool IsDebugMode = true;
#else
    //internal const bool IsDebugMode = false;
    internal const bool IsDebugMode = true;
#endif


    internal const string DomainName = "domain.ir";
    internal const string ApplicationName = "ApplicationName";



    internal const int AccessTokenExpirationMinutes = 10;
}
