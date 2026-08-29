 namespace Alohomora.Core.DataAccess.Contract;

public interface IRepository
{

}

public interface IRepository<TModel, TKey> : IRepository where TModel : IDbTable<TKey>, new()
{

}

public interface IBaseDataRepository<TModel, TKey> : IRepository where TModel : class, IDbEnm<TKey>, new() where TKey : Enum
{
    Task<List<TModel>> GetAll(CancellationToken ct);
}

public interface IFileDataRepository<TModel> : IRepository where TModel : IDbFile, new()
{
    Task<TModel?> GetById(long id, bool enableTracking, CancellationToken ct);
    Task<TModel?> GetByGuid(Guid id, bool enableTracking, CancellationToken ct);
    Task<string> GetFileAddress(long id, CancellationToken ct);
    Task<string> GetFileAddress(Guid guid, CancellationToken ct);


    Task Insert(TModel entity);

    void Delete(TModel entity);
    void DeleteById(long id);
    Task DeleteByGuid(Guid guid);

}

public interface ICrudRepository<TModel> : IRepository where TModel : class, IDbTable<long>, new()
{
    Task<TModel?> GetById(long id, bool enableTracking, CancellationToken ct);
    Task<TModel?> GetByGuid(Guid guid, bool enableTracking, CancellationToken ct);
    Task<long?> GetIdByGuid(Guid guid, CancellationToken ct);

    Task<TModel> Insert(TModel entity);

    void Delete(TModel entity);
    void DeleteById(long id);
    Task DeleteByGuid(Guid guid);
}
