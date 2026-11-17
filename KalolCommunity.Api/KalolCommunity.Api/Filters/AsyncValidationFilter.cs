using KalolCommunity.Contracts.DTO;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace KalolCommunity.Api.Filters
{
    public class AsyncValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get ASP.NET Core Dependency Injection container
            // so we can resolve validators dynamically.
            var serviceProvider = context.HttpContext.RequestServices;

            // Loop through all action parameters sent to the controller.
            // Example:
            // POST api/auth/send-otp
            // parameter => SendOtpRequestDTO
            foreach (var argument in context.ActionArguments.Values)
            {
                // Skip null parameters.
                if (argument is null)
                    continue;

                // Build validator type dynamically.
                //
                // Example: SendOtpRequestDTO
                //
                // becomes: IValidator<SendOtpRequestDTO>
                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

                
                // Resolve validator from Dependency Injection.
                //
                // Example: SendOtpRequestValidator
                var validator = serviceProvider.GetService(validatorType);

                
                // No validator registered for this DTO. Continue to next parameter.
                if (validator is null)
                    continue;

                
                // Use dynamic because validator generic type is only known at runtime.
                dynamic dynamicValidator = validator;


                // Execute FluentValidation asynchronously.
                //
                // Supports:
                // - NotEmpty()
                // - EmailAddress()
                // - MustAsync()
                // - Database validations
                ValidationResult result = await dynamicValidator.ValidateAsync((dynamic)argument);

                
                // If validation failed...
                if (!result.IsValid)
                {
                    // Collect all validation messages.
                    //
                    // Example:
                    // Email is required
                    // Email is invalid
                    var errorMessage = string.Join(" | ",
                        result.Errors.Select(x => x.ErrorMessage));


                    // Create standard API response expected by Angular frontend.
                    var response = new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Data = null
                    };

                    // Stop request pipeline immediately and return HTTP 400.
                    context.Result = new BadRequestObjectResult(response);
                    return;
                }
            }

            // All validations passed. Continue execution to controller action.
            await next();
        }
    }
}
