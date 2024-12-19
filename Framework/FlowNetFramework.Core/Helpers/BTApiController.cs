using FlowNetFramework.Commons.Enums;
using FlowNetFramework.Commons.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowNetFramework.Core.Helpers
{
    public class BTApiController : ControllerBase
    {
        /// <summary>
        /// Resolves BTExceptions to pre-defined HTTP status codes.
        /// </summary>
        /// <param name="exception">BTException</param>
        /// <returns>HTTP result</returns>
        protected ObjectResult BTExceptionResolver(Exception exception)
        {
            if (exception is BTException btException)
            {
                switch (btException.ExceptionType)
                {
                    case ExceptionType.UnhandledError:
                        return Problem(btException.StackTrace, null, StatusCodes.Status500InternalServerError, btException.Message, null);
                    case ExceptionType.UnauthorizedError:
                        return Problem(btException.StackTrace, null, StatusCodes.Status401Unauthorized, btException.Message, null);
                    case ExceptionType.HasNoRecord:
                        return Problem(btException.StackTrace, null, StatusCodes.Status404NotFound, btException.Message, null);
                    case ExceptionType.Duplicate:
                        return Problem(btException.StackTrace, null, StatusCodes.Status409Conflict, btException.Message, null);
                    case ExceptionType.HasRelatedEntites:
                        return Problem(btException.StackTrace, null, StatusCodes.Status424FailedDependency, btException.Message, null);
                    default:
                        return Problem(btException.StackTrace, null, StatusCodes.Status500InternalServerError, btException.Message, null);
                }
            }
            else
            {
                return Problem(exception.StackTrace, null, StatusCodes.Status500InternalServerError, exception.Message, null);
            }
        }
    }
}
