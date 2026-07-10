using System.Text.Json;
using FluentValidation.Results;

namespace Portfolio.Application.Common.Extensions;

public static class ValidationResultExtensions
{
    public static IDictionary<string, string[]> ToValidationDictionary(
        this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(error => JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());
    }
}
