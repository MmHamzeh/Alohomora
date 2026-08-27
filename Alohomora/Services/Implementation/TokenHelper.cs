using Alohomora.Core.Common.Configs;
using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.Services.Implementation;

public class TokenHelper
{
    #region Fields and Ctor

    private const bool useRsa = false;  
    private readonly string SecAlgorithm;

    private readonly string Issuer = ApplicationSetting.DomainName;
    private readonly string Audience = ApplicationSetting.DomainName;

    private readonly RSA? _rsa;
    private readonly byte[]? PrivateKeyPem;
    private readonly byte[]? PublicKeyPem;
    private readonly JwtSecurityTokenHandler TokenHandler;
    private byte[]? _pecretKeyBytes;

    private TokenValidationParameters? _tokenValidationParameters = null;

    public TokenHelper()
    {
        TokenHandler = new();

        SecAlgorithm = useRsa
            ? SecurityAlgorithms.RsaSha256
            : SecurityAlgorithms.HmacSha256;

        if (useRsa)
        {
            _rsa = RSA.Create(2048); // Generate a 2048-bit RSA key

            //Get PrivateKeyPem If Possible
            //Get PublicKeyPem If Possible

            if (PrivateKeyPem is null)
                PrivateKeyPem = _rsa.ExportRSAPrivateKey();
            else
                _rsa.ImportRSAPrivateKey(PrivateKeyPem, out _);

            if (PublicKeyPem is null)
                PublicKeyPem = _rsa.ExportSubjectPublicKeyInfo();
            else
                _rsa.ImportSubjectPublicKeyInfo(PublicKeyPem, out _);

            if (ApplicationSetting.IsDebugMode)
            {
                Console.WriteLine($"private key: {Convert.ToBase64String(PrivateKeyPem, Base64FormattingOptions.InsertLineBreaks)}");
                Console.WriteLine($"public key: {Convert.ToBase64String(PublicKeyPem, Base64FormattingOptions.InsertLineBreaks)}");
            }
        }
        else
        {
            // For symmetric keys, we can use a predefined secret key
            _rsa = null;
            PrivateKeyPem = null; // Not used for symmetric keys
            PublicKeyPem = null;  // Not used for symmetric keys
        }



    }

    #endregion

    internal JwtSecurityToken CreateAccessTokenAsync(Guid userPublicId, IList<string> roles)
    {
        var tokenDescriptor = GetAccessTokenDescriptor(userPublicId, roles);
        return TokenHandler.CreateJwtSecurityToken(tokenDescriptor);
    }

    internal JwtSecurityToken ReadToken(string tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString))
            throw new ArgumentNullException(nameof(tokenString), "Access token cannot be null or empty.");
        var jwtToken = TokenHandler.ReadJwtToken(tokenString);
        if (jwtToken == null)
            throw new SecurityTokenException("Invalid access token format.");
        return jwtToken;
    }

    internal string WriteToken(JwtSecurityToken token)
    {
        return TokenHandler.WriteToken(token);
    }

    internal static RefreshToken CreateRefreshToken(long userId, Guid accessTokenId, bool rememberMe = false)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddDays(rememberMe ? 30 : 1),
            UserId = userId,
            IsRevoked = false,
            RememberMe = rememberMe,
            AccessTokenId = accessTokenId,
            PublicId = Guid.NewGuid()
        };
    }

    internal async Task<bool> ValidateToken(string tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString))
            return false;

        tokenString = tokenString.Trim();

        if (tokenString.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase))
            tokenString = tokenString["Bearer ".Length..];

        var tokenValidationResult = await TokenHandler.ValidateTokenAsync(tokenString, TokenValidationParameters);
        return tokenValidationResult.IsValid;
    }

    public TokenValidationParameters TokenValidationParameters =>
        _tokenValidationParameters ??= new()
        {
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Reduce time skew tolerance

            ValidateIssuer = true,
            ValidIssuer = Issuer,

            ValidateAudience = true,
            ValidAudience = Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecKey,
        };

    // public JwtBearerEvents JwtBearerEvents =>
    //     new()
    //     {
    //         OnAuthenticationFailed = context =>
    //         {
    //             Console.WriteLine("🔴 Authentication failed: " + context.Exception.Message);
    //             return Task.CompletedTask;
    //         },
    //         OnChallenge = context =>
    //         {
    //             Console.WriteLine("🟠 Challenge: " + context.ErrorDescription);
    //             return Task.CompletedTask;
    //         },
    //         OnTokenValidated = context =>
    //         {
    //             Console.WriteLine("🟠 OnTokenValidated: " + context.SecurityToken.Id);
    //             return Task.CompletedTask;
    //         },
    //         OnMessageReceived = context =>
    //         {
    //             Console.WriteLine("🟠 OnMessageReceived: " + context.Token);
    //             return Task.CompletedTask;
    //         },
    //         OnForbidden = context =>
    //         {
    //             Console.WriteLine("🟠 OnForbidden: " + context.Result.Succeeded);
    //             return Task.CompletedTask;
    //         }
    //     };


    internal ClaimsPrincipal GetPrincipalFromExpiredToken(string tokenString)
    {
        var tokenValidationParameters = TokenValidationParameters;
        tokenValidationParameters.ValidateLifetime = false; // Ignore token expiration for this validation

        var principal = TokenHandler.ValidateToken(tokenString, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecToken ||
            !jwtSecToken.Header.Alg.Equals(SecAlgorithm, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");


        return principal;
    }

    public static Guid? GetAccessTokenId(JwtSecurityToken jwtToken)
    {
        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        if (Guid.TryParse(jti, out var accessTokenId) is false)
            throw new SecurityTokenException("Invalid access token");
        return accessTokenId;
    }

    public Guid? GetAccessTokenId(string accessToken)
    {
        var jwtToken = ReadToken(accessToken);
        return GetAccessTokenId(jwtToken);
    }

    public static Guid? GetPublicUserId(JwtSecurityToken jwtToken)
    {
        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
        if (Guid.TryParse(userId, out var publicUserId) is false)
            throw new SecurityTokenException("Invalid access token");
        return publicUserId;
    }

    public Guid? GetPublicUserId(string accessToken)
    {
        var jwtToken = ReadToken(accessToken);
        return GetPublicUserId(jwtToken);
    }

    public static List<string> GetRoles(JwtSecurityToken token)
    {
        return token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(e => e.Value).ToList();
    }

    public List<string> GetRoles(string accessToken)
    {
        var jwtToken = ReadToken(accessToken);
        return GetRoles(jwtToken);
    }

    #region private methods

    private SecurityTokenDescriptor GetAccessTokenDescriptor(Guid userPublicId, IList<string> roles)
    {
        // ایجاد Claims
        //var claims = new Dictionary<string, object>
        var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, userPublicId.ToString() ),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() ),
                new (JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString() ),
                new (JwtRegisteredClaimNames.Iss, Issuer )
            };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));


        // کلید و اعتبارسنجی
        var credentials = new SigningCredentials(SecKey, SecAlgorithm);

        // توکن دسترسی
        return new SecurityTokenDescriptor()
        {
            Issuer = Issuer,
            Audience = Audience,
            //Claims = claims,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(30),
            SigningCredentials = credentials
        };
    }

    private SecurityKey SecKey =>
        useRsa
            ? GetRsaSecurityKey()
            : GetSymmetricSecurityKey();


    private RsaSecurityKey GetRsaSecurityKey()
    {
        return new RsaSecurityKey(_rsa);
    }

    private SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        if (_pecretKeyBytes is not null)
            return new SymmetricSecurityKey(_pecretKeyBytes);

        const string secretKey =
            "A1977D0F-306E-4197-BDAD-FF2000D05CE0-5E5A93A5-4A7C-4EE4-BD9F-D1CD81A4C31D";
        _pecretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        return new SymmetricSecurityKey(_pecretKeyBytes);
    }

    #endregion
}
