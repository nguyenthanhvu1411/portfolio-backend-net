using FluentValidation;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Validators;

public sealed class SkillReorderRequestValidator
    : AbstractValidator<SkillReorderRequest>
{
    public SkillReorderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Danh sách sắp xếp không được để trống.")
            .Must(items => items is { Count: > 0 })
            .WithMessage("Danh sách sắp xếp phải có ít nhất một kỹ năng.")
            .Must(items => items is null || items.Count <= 500)
            .WithMessage("Mỗi lần chỉ được sắp xếp tối đa 500 kỹ năng.")
            .Must(HaveDistinctIds)
            .WithMessage("Danh sách sắp xếp có kỹ năng bị trùng.");

        RuleForEach(x => x.Items)
            .SetValidator(new SkillReorderItemRequestValidator());
    }

    private static bool HaveDistinctIds(IReadOnlyList<SkillReorderItemRequest>? items)
    {
        return items is null || items.Select(x => x.Id).Distinct().Count() == items.Count;
    }
}

public sealed class SkillReorderItemRequestValidator
    : AbstractValidator<SkillReorderItemRequest>
{
    public SkillReorderItemRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id kỹ năng không hợp lệ.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }
}
