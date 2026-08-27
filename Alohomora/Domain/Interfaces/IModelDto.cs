namespace Alohomora.Domain.Interfaces;

internal abstract class IModelDto : IDto
{

    internal DateTime? CreatedOn { get; set; }
    internal Guid? CreatedBy { get; set; }

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}

internal abstract class ICreateModelDto : IDto
{
    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}

internal abstract class IUpdateModelDto : IDto
{

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}
