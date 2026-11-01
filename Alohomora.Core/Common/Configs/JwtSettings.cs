namespace Alohomora.Core.Common.Configs;

public static class JwtSettings
{
    public const string SectionName = "Jwt";

    public static string Issuer { get; set; } = ApplicationConfig.AuthDomain;
    public static string Audience { get; set; } = ApplicationConfig.ApiDomain;
    public static string SecretKey { get; set; } = string.Empty;
    public static bool UseRsa { get; set; } = false;
    public static int AccessTokenExpirationMinutes { get; set; } = ApplicationConfig.AccessTokenExpirationMinutes;
}
