using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Portfolio.Infrastructure.Services.Blogs;

internal static partial class SlugGenerator
{
    public static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd');

        slug = NonAlphaNumericRegex().Replace(slug, "-").Trim('-');
        return MultipleHyphenRegex().Replace(slug, "-");
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphenRegex();
}
