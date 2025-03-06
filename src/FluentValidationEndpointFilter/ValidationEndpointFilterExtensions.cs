using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kimmel.FluentValidationEndpointFilter;

public static class ValidationEndpointFilterExtensions
{
    /// <summary>
    /// This method will set up a ValidationEndpointFilter to check your
    /// model of type T with any Validators that have been registered for
    /// the type. It also uses Produces to let OpenAPI know of potential
    /// return types.
    /// </summary>
    /// <typeparam name="T">Type to validate</typeparam>
    /// <param name="builder">RouteHandlerBuiler is being extended with this Validate method</param>
    /// <returns>RouteHandlerBuilder so you can chain additional filters</returns>
    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationEndpointFilter<T>>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity);
}
