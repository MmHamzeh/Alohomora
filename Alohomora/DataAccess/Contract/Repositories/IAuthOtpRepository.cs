namespace Alohomora.DataAccess.Contract.Repositories;

internal interface IAuthOtpRepository : IRepository<AuthOtp, long>
{
    Task AddAsync(AuthOtp authOtp, CancellationToken ct);
    Task<AuthOtp?> GetValidByPhoneNumberAndCode(string phoneNumber, string code, CancellationToken ct);
    Task<AuthOtp?> GetValidByPhoneNumber(string phoneNumber, CancellationToken ct);
    Task<AuthOtp?> GetByEmailAndCode(string email, string code, CancellationToken ct);
    Task<AuthOtp?> GetNotUseByUserPublicId(Guid PublicId, CancellationToken ct);
}
