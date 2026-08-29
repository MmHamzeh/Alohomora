using Alohomora.Core.DataAccess.Contract.Repositories;

namespace Alohomora.Core.DataAccess.Contract;

public interface IUnitOfWork
{
    #region Repositories


    IRefreshTokenRepository RefreshTokenRepository { get; }
    IAuthOtpRepository AuthOtpRepository { get; }
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    IUserRoleRepository UserRoleRepository { get; }



    #endregion





    #region Methods

    Task<int> SaveChanges();
    void RejectChanges();

    #endregion
}