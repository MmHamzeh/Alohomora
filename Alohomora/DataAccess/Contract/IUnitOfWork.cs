namespace Alohomora.DataAccess.Contract;

internal interface IUnitOfWork
{
    #region Repositories


    internal IRefreshTokenRepository RefreshTokenRepository { get; }
    internal IAuthOtpRepository AuthOtpRepository { get; }
    internal IUserRepository UserRepository { get; }
    internal IRoleRepository RoleRepository { get; }
    internal IUserRoleRepository UserRoleRepository { get; }



    #endregion





    #region Methods

    Task<int> SaveChanges();
    void RejectChanges();

    #endregion
}