using FluentValidation;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Validators;

public sealed class SkillUpdateRequestValidator
    : AbstractValidator<SkillUpdateRequest>
{
    public SkillUpdateRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Nhóm kỹ năng không hợp lệ.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Tên kỹ năng không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên kỹ năng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Level)
            .Must(level => !level.HasValue || Enum.IsDefined(level.Value))
            .WithMessage("Cấp độ kỹ năng không hợp lệ.");

        RuleFor(x => x.IconUrl)
            .MaximumLength(500)
            .WithMessage("Đường dẫn icon không được vượt quá 500 ký tự.")
            .Must(BeValidUrlOrRelativePath)
            .WithMessage("Đường dẫn icon không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.IconUrl));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }

    private static bool BeValidUrlOrRelativePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith('/') || trimmed.StartsWith("./"))
        {
            return true;
        }

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
