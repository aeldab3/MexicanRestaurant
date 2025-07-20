using MexicanRestaurant.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MexicanRestaurant.Application.Helpers
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred.");

            var isApiRequest = context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json");
            string userMessage = "An unexpected error occurred.";
            int statusCode = StatusCodes.Status500InternalServerError;

            if (context.Exception is ProductNotFoundException ||
                context.Exception is ApplicationException)
            {
                userMessage = context.Exception.Message;
                statusCode = StatusCodes.Status404NotFound;
            }

            if (isApiRequest)
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = userMessage,
                    error = context.Exception.Message
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            else
            {
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = context.HttpContext.TraceIdentifier,
                    Message = userMessage
                };

                context.HttpContext.Items["GlobalErrorMessage"] = userMessage;
                var result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary<ErrorViewModel>(
                               new EmptyModelMetadataProvider(),
                               context.ModelState)
                    {
                        Model = errorViewModel,
                        ["Exception"] = context.Exception,
                        ["Message"] = userMessage
                    }
                };
                context.Result = result;
            }
            context.ExceptionHandled = true;   
        }
    }
}
