using FlowNetFramework.Commons.Constants;
using FlowNetFramework.Commons.Models;
using FlowNetFramework.Commons.Models.Responses;

namespace FFlowNetFramework.Commons.Models.Responses;

public class ValidationErrorResponse : ServiceResponse
{
    public List<ValidationError> Errors { get; set; }

    public ValidationErrorResponse(List<ValidationError> errors)
    {
        Success = false;
        Messages = new List<string>() { ResponseMessage.InvalidRequest };
        Errors = errors;
    }

    public static ValidationErrorResponse Create(List<ValidationError> errors)
    {
        return new ValidationErrorResponse(errors);
    }
}
