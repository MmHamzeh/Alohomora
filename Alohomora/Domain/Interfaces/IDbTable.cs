namespace Alohomora.Domain.Interfaces;

internal interface IDbTable
{
    internal Guid PublicId { get; set; }
}


internal interface IDbTable<T> : IDbTable
{
    internal T Id { get; set; }
}
