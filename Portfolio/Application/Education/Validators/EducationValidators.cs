using FluentValidation;
using Portfolio.Application.Education.DTOs;

namespace Portfolio.Application.Education.Validators;

public sealed class EducationCreateRequestValidator : AbstractValidator<EducationCreateRequest>
{
    public EducationCreateRequestValidator() => ApplyRules(this);

    internal static void ApplyRules<T>(AbstractValidator<T> validator)
        where T : EducationCreateRequest
    {
        var maxYear = DateTime.UtcNow.Year + 10;

        validator.RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("Tên trường không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.Major)
            .NotEmpty().WithMessage("Chuyên ngành không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.Degree).MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Degree));
        validator.RuleFor(x => x.StartYear)
            .InclusiveBetween(1900, maxYear)
            .When(x => x.StartYear.HasValue);
        validator.RuleFor(x => x.EndYear)
            .InclusiveBetween(1900, maxYear)
            .When(x => x.EndYear.HasValue);
        validator.RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .WithMessage("Năm kết thúc phải lớn hơn hoặc bằng năm bắt đầu.")
            .When(x => x.StartYear.HasValue && x.EndYear.HasValue);
        validator.RuleFor(x => x.GPA).MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.GPA));
        validator.RuleFor(x => x.Description).MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        validator.RuleFor(x => x.LogoUrl).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
    }
}

public sealed class EducationUpdateRequestValidator : AbstractValidator<EducationUpdateRequest>
{
    public EducationUpdateRequestValidator() => EducationCreateRequestValidator.ApplyRules(this);
}
