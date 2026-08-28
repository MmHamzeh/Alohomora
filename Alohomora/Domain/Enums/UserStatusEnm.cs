namespace Alohomora.Core.Domain.Enums;

public enum UserStatusEnm
{
    /// <summary>
    /// نامعتبر
    /// </summary>
    [Description("نامعتبر")]
    Invalid = 0,

    /// <summary>
    /// فعال
    /// </summary>
    [Description("فعال")]
    Active = 1,

    /// <summary>
    /// غیر فعال
    /// </summary>
    [Description("غیر فعال")]
    DeActive = 2,

    /// <summary>
    /// قفل شده
    /// </summary>
    [Description("قفل شده")]
    Deleted = 3
}
