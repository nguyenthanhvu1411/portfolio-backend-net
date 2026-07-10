using FluentValidation;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Validators;

public sealed class SkillCategoryUpdateRequestValidator
    : AbstractValidator<SkillCategoryUpdateRequest>
{
    public SkillCategoryUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Tên nhóm kỹ năng không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên nhóm kỹ năng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }
}
