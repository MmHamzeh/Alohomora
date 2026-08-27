namespace Alohomora.Core.Domain.Models.ResponseModels;

public interface ISingleResponse<TModel> : IResponse where TModel : class, IVm, new()
{
    public TModel Model { get; set; }
}

public class SingleResponse<TModel> : ISingleResponse<TModel> where TModel : class, IVm, new()
{

    #region Ctor

    internal SingleResponse()
    {

    }

    internal SingleResponse(TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Model = model;
        StatusCode = statusCode;
        HasError = false;
        ExceptionMessage = string.Empty;
    }

    internal SingleResponse(Exception exception, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        Model = null;
        StatusCode = statusCode;
        HasError = true;
        ExceptionMessage = exception.Message;
    }

    internal SingleResponse(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        Model = null;
        StatusCode = statusCode;
        HasError = true;
        Message = errorMessage;
    }

    #endregion

    public string? Message { get; set; }
    public bool HasError { get; set; }
    public string? ExceptionMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    
    public TModel? Model { get; set; }
}

