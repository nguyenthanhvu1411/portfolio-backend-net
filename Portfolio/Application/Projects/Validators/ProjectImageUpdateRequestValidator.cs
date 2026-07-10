using FluentValidation;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Validators;

public sealed class ProjectImageUpdateRequestValidator
    : AbstractValidator<ProjectImageUpdateRequest>
{
    public ProjectImageUpdateRequestValidator()
    {
        RuleFor(x => x.Caption)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Caption));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
