using FlowNetFramework.Commons.Constants;
using System.Net;
using System.Text.Json.Serialization;

namespace FlowNetFramework.Commons.Models.Responses;

public class ServiceResponse
{
    public bool Success { get; init; }
    public List<string> Messages { get; init; }
    [JsonIgnore]
    public int StatusCode { get; set; }
    public object Payload { get; init; }

    public static ServiceResponse CreateSuccess(string message)
    {
        return CreateServiceResponse(true, message, (int)HttpStatusCode.OK, null);
    }

    public static ServiceResponse CreateSuccess<T>(string message, T data)
    {
        return CreateServiceResponseObject(true, (int)HttpStatusCode.OK, message, data);
    }

    public static ServiceResponse CreateSuccess(string message, string data)
    {
        return CreateServiceResponse(true, message, (int)HttpStatusCode.OK, data);
    }

    public static ServiceResponse CreateExceptionError()
    {
        return CreateServiceResponse(false, ResponseMessage.ServerError, (int)HttpStatusCode.InternalServerError, null);
    }

    public static ServiceResponse CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        if (string.IsNullOrEmpty(message))
        {
            message = ResponseMessage.ServerError;
        }

        return CreateServiceResponse(false, message, (int)HttpStatusCode.InternalServerError, null);
    }

    private static ServiceResponse CreateServiceResponse(bool success, string message, int? statusCode, string data)
    {
        return new ServiceResponse
        {
            StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError,
            Success = success,
            Messages = new List<string>() { message },
            Payload = data
        };
    }

    private static ServiceResponse CreateServiceResponseObject<T>(bool success, int? statusCode, string message, T data)
    {
        return new ServiceResponse
        {
            StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError,
            Success = success,
            Messages = new List<string>() { message },
            Payload = data
        };
    }

    public static ServiceResponse CreateConflict(string incomingMessage, HttpStatusCode statusCode)
    {
        string message = string.IsNullOrEmpty(incomingMessage) ? ResponseMessage.Conflict : incomingMessage;

        return CreateServiceResponse(false, message, (int)HttpStatusCode.Conflict, null);
    }

    public static ServiceResponse CreateNotFound(string incomingMessage, HttpStatusCode statusCode)
    {
        string message = string.IsNullOrEmpty(incomingMessage) ? ResponseMessage.NotFound : incomingMessage;

        return CreateServiceResponse(false, message, (int)HttpStatusCode.NotFound, null);
    }

    public static ServiceResponse CreateForbidden(string incomingMessage, HttpStatusCode statusCode)
    {
        string message = string.IsNullOrEmpty(incomingMessage) ? ResponseMessage.Forbidden : incomingMessage;

        return CreateServiceResponse(false, message, (int)HttpStatusCode.Forbidden, null);
    }
}
