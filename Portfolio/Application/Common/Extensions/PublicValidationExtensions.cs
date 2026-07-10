using FluentValidation;
using Portfolio.Application.Common.Extensions;
using Portfolio.Common.Exceptions;

namespace Portfolio.Application.Common.Extensions;

public static class PublicValidationExtensions
{
    public static async Task ValidateRequestAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(
            instance,
            cancellationToken);

        if (!result.IsValid)
        {
            throw new RequestValidationException(
                result.ToValidationDictionary());
        }
    }
}

