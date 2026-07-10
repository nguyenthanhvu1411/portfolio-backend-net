using FluentValidation;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Validators;

public sealed class SkillFilterRequestValidator
    : AbstractValidator<SkillFilterRequest>
{
    public SkillFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(100)
            .WithMessage("Từ khóa không được vượt quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Nhóm kỹ năng không hợp lệ.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Số bản ghi mỗi trang phải từ 1 đến 100.");
    }
}
