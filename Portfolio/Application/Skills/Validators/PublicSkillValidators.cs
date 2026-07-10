using FluentValidation;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Common.Models;

namespace Portfolio.Application.Skills.Validators;

public sealed class PublicSkillFilterRequestValidator
    : AbstractValidator<PublicSkillFilterRequest>
{
    public PublicSkillFilterRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId phải lớn hơn 0.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(100)
            .WithMessage("Từ khóa không được vượt quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
    }
}

public sealed class PublicLimitRequestValidator
    : AbstractValidator<PublicLimitRequest>
{
    public PublicLimitRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 50)
            .WithMessage("Limit phải nằm trong khoảng từ 1 đến 50.");
    }
}

