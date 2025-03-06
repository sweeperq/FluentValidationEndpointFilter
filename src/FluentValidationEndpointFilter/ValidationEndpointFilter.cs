using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kimmel.FluentValidationEndpointFilter;

/// <summary>
/// This class utilizes FluentValidation to perform validation
/// on the model being sent to a Minimal API. This is helpful to
/// reduce the amount of code in your Minimal API method definitions.
/// Note: Validators should be injected with DI via IServiceCollection
/// </summary>
/// <typeparam name="T">The type of model being validated</typeparam>
/// <param name="validator">The validator to use for validation</param>
public class ValidationEndpointFilter<T>(IEnumerable<IValidator<T>> validators) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Nothing to do if there is no validator present
        if (!validators.Any())
        {
            return await next(context);
        }

        // Get the first model of type T. If there isn't one, assume the body was missing in the request.
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            return TypedResults.BadRequest("Request body is missing and cannot be validated");    
        }

        // Validate the model using any validators for this model
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        // If there are any errors, return a ValidationProblem (Status 422)
        if (failures.Any())
        {
            var details = new ValidationProblemDetails(failures.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage }));
            return TypedResults.UnprocessableEntity(details);
        }
        

        // No problems, continue to next action in the endpoint pipeline
        return await next(context);
    }
}
