namespace Alohomora.Domain.Interfaces;

internal interface IDbModel : IDbTable<long>
{
    internal DateTime CreatedOn { get; set; }
    internal Guid CreatedBy { get; set; }

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }

    internal bool IsDeleted { get; set; }

}
