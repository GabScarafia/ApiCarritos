using FluentValidation;

namespace Api.Extensions;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Buscamos en los argumentos del endpoint el objeto que coincida con el tipo T
        var argument = context.Arguments.FirstOrDefault(x => x is T) as T;

        if (argument is not null)
        {
            var validationResult = await _validator.ValidateAsync(argument);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
        }
        return await next(context);
    }


}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}