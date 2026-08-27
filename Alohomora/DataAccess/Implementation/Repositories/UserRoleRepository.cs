namespace Alohomora.DataAccess.Implementation.Repositories;

internal class UserRoleRepository : IUserRoleRepository
{
    #region Fields and Ctor

    private readonly DatabaseContext _dbContext;
    private readonly DatabaseContextRead _authDatabaseContextRead;

    internal UserRoleRepository(DatabaseContext? dbContext = null, DatabaseContextRead? authDataBaseContextRead = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _authDatabaseContextRead = authDataBaseContextRead ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion




}