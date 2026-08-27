namespace Alohomora.Domain.Models.DbModels;

internal class User : IDbTable<long>
{
    #region IDbTableProperties

    public long Id { get; set; }
    public Guid PublicId { get; set; }

    #endregion

    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string PasswordHash { get; set; }
    public bool CanUsePassword { get; set; }
    public DateTime CreatedOn { get; set; }


    #region Relations

    public UserStatusEnm UserStatusId { get; set; }
    public UserStatus UserStatus { get; set; }


    // ارتباط با نقش‌ها
    public ICollection<UserRole> UserRoles { get; set; }

    // برای Refresh Token
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<AuthOtp> AuthOtps { get; set; }


    #endregion
}
