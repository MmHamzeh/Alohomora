namespace Alohomora.Core.Domain.Interfaces;

public interface IDbTable
{
    internal Guid PublicId { get; set; }
}


public interface IDbTable<T> : IDbTable
{
    internal T Id { get; set; }
}
