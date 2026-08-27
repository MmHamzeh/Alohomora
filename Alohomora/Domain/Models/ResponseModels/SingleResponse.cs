namespace Alohomora.Core.Domain.Models.ResponseModels;

public interface ISingleResponse<TModel> : IResponse 
{
    public TModel Model { get; set; }
    public new bool Success { get; }
}

public class SingleResponse<TModel> : ISingleResponse<TModel> 
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
        Model = default;
        StatusCode = statusCode;
        HasError = true;
        ExceptionMessage = exception.Message;
    }

    internal SingleResponse(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        Model = default;
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
    
    public bool Success => !HasError;
}

