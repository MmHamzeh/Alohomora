using Alohomora.Core.Common.Configs;
using Alohomora.Core.Domain.Models.DbModels;

namespace Alohomora.Core.Services.Implementation;

public class TokenHelper : IDisposable
{
    #region Fields and Ctor

    private readonly bool useRsa;
    private readonly string _secAlgorithm;

    private readonly JwtSettings _settings;
    private readonly string _issuer;
    private readonly string _audience;

    private readonly RSA? _rsa;
    private readonly string _privateKeyPem;
    private readonly string _publicKeyPem;
    private readonly JwtSecurityTokenHandler TokenHandler;
    private byte[]? _secretKeyBytes;

    private readonly string _privateKeyFilePath;
    private readonly string _publicKeyFilePath;
    private readonly TimeProvider _timeProvider;

    private TokenValidationParameters? _tokenValidationParameters = null;

    public TokenHelper(IOptions<JwtSettings> options, TimeProvider timeProvider)
    {
        _settings = options.Value;
        useRsa = _settings.UseRsa;
        _issuer = _settings.Issuer;
        _audience = _settings.Audience;
        _timeProvider = timeProvider;

        TokenHandler = new();

        _secAlgorithm = useRsa
            ? SecurityAlgorithms.RsaSha256
            : SecurityAlgorithms.HmacSha256;

        if (useRsa)
        {
            var rsaKeysFullDirectory = Path.Combine(AppContext.BaseDirectory, ApplicationSetting.RsaKeysDirectory);

            if (!Directory.Exists(rsaKeysFullDirectory))
            {
                Directory.CreateDirectory(rsaKeysFullDirectory);
            }

            _privateKeyFilePath = Path.Combine(rsaKeysFullDirectory, "private_key.pem");
            _publicKeyFilePath = Path.Combine(rsaKeysFullDirectory, "public_key.pem");

            if (File.Exists(_privateKeyFilePath) && File.Exists(_publicKeyFilePath))
            {
                _privateKeyPem = File.ReadAllText(_privateKeyFilePath);
                _publicKeyPem = File.ReadAllText(_publicKeyFilePath);

                _rsa = RSA.Create();
                _rsa.ImportFromPem(_privateKeyPem);

                if (!PublicKeysMatch(_rsa, _publicKeyPem))
                {
                    _rsa.Dispose();

                    throw new CryptographicException(
                        "RSA key mismatch. Refusing to start.");
                }
            }
            else
            {
                // Generate a 2048-bit RSA key
                _rsa = RSA.Create(2048);

                _privateKeyPem = _rsa.ExportPkcs8PrivateKeyPem();
                _publicKeyPem = _rsa.ExportSubjectPublicKeyInfoPem();

                File.WriteAllText(_privateKeyFilePath, _privateKeyPem);
                File.WriteAllText(_publicKeyFilePath, _publicKeyPem);
            }

            if (ApplicationSetting.IsDebugMode)
            {
                Console.WriteLine($"[RSA Private Key Loaded]:\n{_privateKeyPem}");
                Console.WriteLine($"[RSA Public Key Loaded]:\n{_publicKeyPem}");
            }

        }
        else
        {
            // For symmetric keys, we can use a predefined secret key
            _rsa = null;
            _privateKeyPem = string.Empty; // Not used for symmetric keys
            _publicKeyPem = string.Empty;  // Not used for symmetric keys
            _privateKeyFilePath = string.Empty;
            _publicKeyFilePath = string.Empty;
        }
    }

    #endregion

    internal JwtSecurityToken CreateAccessToken(Guid userPublicId, IList<string> roles)
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

    internal static RefreshToken CreateRefreshToken(long userId, Guid accessTokenId, TimeProvider timeProvider, bool rememberMe = false)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = timeProvider.GetUtcNow().DateTime.AddDays(rememberMe ? 30 : 1),
            UserId = userId,
            IsRevoked = false,
            RememberMe = rememberMe,
            AccessTokenId = accessTokenId,
            PublicId = Guid.CreateVersion7()
        };
    }

    internal async Task<bool> ValidateToken(string tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString))
            return false;

        tokenString = tokenString.Trim();

        if (tokenString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
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
            ValidIssuer = _issuer,

            ValidateAudience = true,
            ValidAudience = _audience,

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
        var tokenValidationParameters = TokenValidationParameters.Clone();
        tokenValidationParameters.ValidateLifetime = false; // Ignore token expiration for this validation

        var principal = TokenHandler.ValidateToken(tokenString, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecToken ||
            !jwtSecToken.Header.Alg.Equals(_secAlgorithm, StringComparison.OrdinalIgnoreCase))
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
                new (JwtRegisteredClaimNames.Sub, userPublicId.ToString()),
                new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new (JwtRegisteredClaimNames.Iat, _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new (JwtRegisteredClaimNames.Iss, _issuer)
            };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));


        // کلید و اعتبارسنجی
        var credentials = new SigningCredentials(SecKey, _secAlgorithm);

        // توکن دسترسی
        return new SecurityTokenDescriptor()
        {
            Issuer = _issuer,
            Audience = _audience,
            Subject = new ClaimsIdentity(claims),
            Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(_settings.AccessTokenExpirationMinutes),
            SigningCredentials = credentials
        };
    }

    private SecurityKey SecKey =>
        useRsa
            ? GetRsaSecurityKey()
            : GetSymmetricSecurityKey();


    private RsaSecurityKey GetRsaSecurityKey()
    {
        if (_rsa == null)
            throw new InvalidOperationException("RSA is not initialized.");

        return new RsaSecurityKey(_rsa);
    }

    private SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        if (_secretKeyBytes is not null)
            return new SymmetricSecurityKey(_secretKeyBytes);

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey is not configured.");

        _secretKeyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
        return new SymmetricSecurityKey(_secretKeyBytes);
    }

    private static bool PublicKeysMatch(RSA rsa, string publicKeyPem)
    {
        try
        {
            using var storedPublicKey = RSA.Create();
            storedPublicKey.ImportFromPem(publicKeyPem);

            byte[] derivedPublicKey = rsa.ExportSubjectPublicKeyInfo();
            byte[] storedPublicKeyBytes = storedPublicKey.ExportSubjectPublicKeyInfo();

            return CryptographicOperations.FixedTimeEquals(derivedPublicKey, storedPublicKeyBytes);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    #endregion

    public void Dispose()
    {
        _rsa?.Dispose();
        GC.SuppressFinalize(this);
    }
}
