namespace Alohomora.Sql.Repositories;

internal class AuthOtpRepository : IAuthOtpRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;

    internal AuthOtpRepository(DatabaseContext? dbContext = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion

    public async Task AddAsync(AuthOtp authOtp, CancellationToken ct)
    {
        await _dbContext.AuthOtps.AddAsync(authOtp, ct);
    }

    public async Task<AuthOtp?> GetValidByPhoneNumberAndCode(string phoneNumber, string code, CancellationToken ct)
    {
        if (PhoneNumberHelper.IsValidPhoneNumber(phoneNumber) is false)
            return null;

        return await _dbContext.AuthOtps
            .FirstOrDefaultAsync(e => e.UserPhoneNumber == phoneNumber
                                      && e.Expires > DateTime.Now
                                      && e.Code == code
                                      && e.IsUsed == false, ct);
    }

    public async Task<AuthOtp?> GetValidByPhoneNumber(string phoneNumber, CancellationToken ct)
    {
        return await _dbContext.AuthOtps
            .FirstOrDefaultAsync(e => e.UserPhoneNumber == phoneNumber && e.Expires >= DateTime.Now && e.IsUsed == false, ct);
    }

    public async Task<AuthOtp?> GetByEmailAndCode(string email, string code, CancellationToken ct)
    {
        return await _dbContext.AuthOtps
            .FirstOrDefaultAsync(e => e.UserEmail == email && e.Code == code && e.IsUsed == false, ct);
    }

    public async Task<AuthOtp?> GetNotUseByUserPublicId(Guid publicId, CancellationToken ct)
    {
        return await _dbContext.AuthOtps
            .FirstOrDefaultAsync(e => e.User.PublicId == publicId && e.IsUsed == false, ct);
    }

}


