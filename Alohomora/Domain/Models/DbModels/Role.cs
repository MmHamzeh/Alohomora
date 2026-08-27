namespace Alohomora.Domain.Models.DbModels;

internal class Role : IDbTable<long>
{
    #region Ctor

    public Role()
    {

    }

    public Role(string name, string faName) : this()
    {
        Name = name;
        FaName = faName;
    }

    #endregion

    #region IDbTableProperties

    public long Id { get; set; }
    public Guid PublicId { get; set; }

    #endregion

    public string Name { get; set; }
    public string FaName { get; set; }
    public string Description { get; set; }

    #region Relations

    public ICollection<UserRole> UserRoles { get; set; }

    #endregion
}
