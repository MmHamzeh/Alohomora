namespace Alohomora.Common.Helpers;

internal static class OtpHelper
{
    #region Fields and Ctor

    private static readonly Random _rand;

    static OtpHelper()
    {
        _rand = new Random();
    }

    #endregion

    /// <summary>
    /// Generates a random 6-digit number
    /// </summary>
    /// <returns></returns>
    internal static string GenerateAuthOtp()
    {
        var otp = _rand.Next(100_000, 999_999);
        return otp.ToString();
    }

}