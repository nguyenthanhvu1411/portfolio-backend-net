using FluentValidation;
using Portfolio.Application.Experiences.DTOs;

namespace Portfolio.Application.Experiences.Validators;

public sealed class ExperienceFilterRequestValidator : AbstractValidator<ExperienceFilterRequest>
{
    public ExperienceFilterRequestValidator()
    {
        RuleFor(x => x.Keyword).MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
    }
}

public sealed class ExperienceCreateRequestValidator : AbstractValidator<ExperienceCreateRequest>
{
    public ExperienceCreateRequestValidator() => ApplyRules(this);

    internal static void ApplyRules<T>(AbstractValidator<T> validator)
        where T : ExperienceCreateRequest
    {
        validator.RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Vị trí công việc không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.Company)
            .NotEmpty().WithMessage("Tên công ty không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.CompanyLogoUrl).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyLogoUrl));
        validator.RuleFor(x => x.Location).MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
        validator.RuleFor(x => x.StartDate).NotEmpty();
        validator.RuleFor(x => x.EndDate)
            .Null().WithMessage("Công việc hiện tại không được có ngày kết thúc.")
            .When(x => x.IsCurrent);
        validator.RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.")
            .When(x => !x.IsCurrent && x.EndDate.HasValue);
        validator.RuleFor(x => x.Description).MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        validator.RuleFor(x => x.Technologies).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Technologies));
        validator.RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class ExperienceUpdateRequestValidator : AbstractValidator<ExperienceUpdateRequest>
{
    public ExperienceUpdateRequestValidator() =>
        ExperienceCreateRequestValidator.ApplyRules(this);
}
