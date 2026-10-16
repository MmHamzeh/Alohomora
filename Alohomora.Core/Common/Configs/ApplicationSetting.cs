namespace Alohomora.Core.Common.Configs;

public static class ApplicationSetting
{
#if DEBUG
    public const bool IsDebugMode = true;
#else
    //public const bool IsDebugMode = false;
    public const bool IsDebugMode = true;
#endif

    // When true the API responses may include the OTP for testing/debugging.
    // Should be false in production to avoid leaking OTP codes.
    public const bool ExposeOtpInResponse = IsDebugMode;


    public const string RsaKeysDirectory = "rsa-keys";

    public const int AccessTokenExpirationMinutes = 10;
    public const int OtpExpirationMinutes = 5;

}
