namespace Alohomora.Core.Domain.Models.ResponseModels;

public interface IListResponse<TModel> : IResponse where TModel : class, IVm, new()
{
    public List<TModel> ModelList { get; set; }
}

public class ListResponse<TModel> : IListResponse<TModel> where TModel : class, IVm, new()
{
    #region Ctor

    internal ListResponse()
    {

    }

    internal ListResponse(List<TModel> modelList, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ModelList = modelList;
        StatusCode = statusCode;
        HasError = false;
        ExceptionMessage = string.Empty;
    }

    internal ListResponse(Exception exception, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        ModelList = null;
        StatusCode = statusCode;
        HasError = true;
        ExceptionMessage = exception.Message;
    }

    #endregion

    public string? Message { get; set; }
    public bool HasError { get; set; }
    public string? ExceptionMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    
    public List<TModel>? ModelList { get; set; }
}

