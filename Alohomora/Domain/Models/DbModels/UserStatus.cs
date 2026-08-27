namespace Alohomora.Domain.Models.DbModels;

internal class UserStatus : IDbEnm<UserStatusEnm>
{
    #region IDbEnm Properties

    public Guid PublicId { get; set; }
    public UserStatusEnm Id { get; set; }
    public string Title { get; set; }
    public string TitleEn { get; set; }
    public string Description { get; set; }

    #endregion




    #region Relations

    public ICollection<User> Users { get; set; }


    #endregion
}