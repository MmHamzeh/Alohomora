namespace Alohomora.Core.Domain.Models.DbModels;

public class UserRole : IDbTable<long>
{
    #region IDbTableProperties

    public long Id { get; set; }
    public Guid PublicId { get; set; }

    #endregion



    #region Relations

    public User User { get; set; }
    public long UserId { get; set; }

    public Role Role { get; set; }
    public long RoleId { get; set; }

    #endregion


}