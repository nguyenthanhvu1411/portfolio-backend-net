using FluentValidation;
using Portfolio.Application.Settings.DTOs;

namespace Portfolio.Application.Settings.Validators;

public sealed class SettingUpdateRequestValidator : AbstractValidator<SettingUpdateRequest>
{
    public SettingUpdateRequestValidator()
    {
        RuleFor(x => x.SiteName)
            .NotEmpty().WithMessage("Tên website không được để trống.")
            .MaximumLength(200);

        RuleFor(x => x.ThemeColor)
            .MaximumLength(30)
            .Matches(@"^(#[0-9A-Fa-f]{3,8}|[a-zA-Z]+)$")
            .WithMessage("Màu chủ đạo phải là mã HEX hoặc tên màu hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.ThemeColor));

        RuleFor(x => x.SeoTitle).MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.SeoTitle));
        RuleFor(x => x.SeoDescription).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.SeoDescription));
        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Email liên hệ không đúng định dạng.")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.FooterText).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.FooterText));
    }
}
