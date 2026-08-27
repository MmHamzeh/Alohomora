namespace Alohomora.Core.Domain.Models.ResponseModels;

public interface IListResponse<TModel> : IResponse 
{
    public List<TModel> ModelList { get; set; }
}

public class ListResponse<TModel> : IListResponse<TModel> 
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
    public bool Success { get; }

    public List<TModel>? ModelList { get; set; }
}

