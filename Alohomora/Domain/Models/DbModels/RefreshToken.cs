namespace Alohomora.Domain.Models.DbModels;

internal class RefreshToken : IDbTable<long>
{
    #region IDbTableProperties

    public long Id { get; set; }
    public Guid PublicId { get; set; }

    #endregion

    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public bool RememberMe { get; set; }
    public Guid AccessTokenId { get; set; }

    #region Relations

    public User User { get; set; }
    public long UserId { get; set; }

    #endregion
}
