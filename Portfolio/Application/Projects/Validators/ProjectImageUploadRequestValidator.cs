using FluentValidation;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Validators;

public sealed class ProjectImageUploadRequestValidator
    : AbstractValidator<ProjectImageUploadRequest>
{
    public ProjectImageUploadRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Vui lòng chọn ảnh dự án.");

        RuleFor(x => x.Caption)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Caption));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
