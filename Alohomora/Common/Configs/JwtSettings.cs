namespace Alohomora.Core.Common.Configs;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = ApplicationSetting.DomainName;
    public string Audience { get; set; } = ApplicationSetting.DomainName;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseRsa { get; set; } = false;
    public int AccessTokenExpirationMinutes { get; set; } = ApplicationSetting.AccessTokenExpirationMinutes;
}
