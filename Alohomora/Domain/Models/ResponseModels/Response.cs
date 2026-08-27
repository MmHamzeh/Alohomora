namespace Alohomora.Core.Domain.Models.ResponseModels;

public interface IResponse
{
    public string? Message { get; set; }
    public bool HasError { get; set; }
    public string? ExceptionMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}



public class Response : IResponse
{
    internal Response()
    {
        StatusCode = HttpStatusCode.OK;
    }

    internal Response(Exception exception, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        StatusCode = statusCode;
        HasError = true;
        ExceptionMessage = exception.Message;
    }

    internal Response(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        StatusCode = statusCode;
        HasError = true;
        ExceptionMessage = errorMessage;
    }

    internal Response(bool hasError, HttpStatusCode statusCode)
    {
        this.HasError = hasError;
        this.StatusCode = statusCode;

    }

    public string? Message { get; set; }
   
    public bool HasError { get; set; }
    public string? ExceptionMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
