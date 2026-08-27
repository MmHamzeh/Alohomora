namespace Alohomora.Core.Domain.Interfaces;

public abstract class IModelDto : IDto
{

    internal DateTime? CreatedOn { get; set; }
    internal Guid? CreatedBy { get; set; }

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}

public abstract class ICreateModelDto : IDto
{
    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}

public abstract class IUpdateModelDto : IDto
{

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}
