using FluentValidation;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Validators;

public sealed class ProjectFilterRequestValidator
    : AbstractValidator<ProjectFilterRequest>
{
    public ProjectFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
