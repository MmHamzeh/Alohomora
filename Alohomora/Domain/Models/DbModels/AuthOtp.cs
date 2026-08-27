namespace Alohomora.Core.Domain.Models.DbModels;

public class AuthOtp : IDbTable<long>
{
    #region IDbTableProperties

    public long Id { get; set; }
    public Guid PublicId { get; set; }

    #endregion

    public string Code { get; set; }
    public DateTime Expires { get; set; }
    public bool IsUsed { get; set; }
    public string? UserPhoneNumber { get; set; }
    public string? UserEmail { get; set; }

    #region Relations

    public User? User { get; set; }
    public long? UserId { get; set; }

    #endregion
}