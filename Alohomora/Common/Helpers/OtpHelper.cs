namespace Alohomora.Core.Common.Helpers;

internal static class OtpHelper
{
    #region Fields and Ctor

    private const int OtpMinimum = 100_000;
    private const int OtpMaximumExclusive = 1_000_000;

    #endregion

    internal static string GenerateAuthOtp()
    {
        return RandomNumberGenerator
            .GetInt32(OtpMinimum, OtpMaximumExclusive)
            .ToString("D6");
    }

}