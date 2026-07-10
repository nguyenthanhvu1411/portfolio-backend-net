using System.Text.RegularExpressions;
using FluentValidation;
using Portfolio.Application.Contacts.DTOs;

namespace Portfolio.Application.Contacts.Validators;

public sealed partial class PublicContactMessageCreateRequestValidator
    : AbstractValidator<PublicContactMessageCreateRequest>
{
    public PublicContactMessageCreateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Họ tên không được để trống.")
            .MaximumLength(200)
            .WithMessage("Họ tên không được vượt quá 200 ký tự.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email không được để trống.")
            .EmailAddress()
            .WithMessage("Email không đúng định dạng.")
            .MaximumLength(255)
            .WithMessage("Email không được vượt quá 255 ký tự.");

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .WithMessage("Số điện thoại không được vượt quá 50 ký tự.")
            .Matches(@"^[0-9+\-\s().]*$")
            .WithMessage("Số điện thoại chứa ký tự không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Company)
            .MaximumLength(200)
            .WithMessage("Tên công ty không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Company));

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(250)
            .WithMessage("Tiêu đề không được vượt quá 250 ký tự.")
            .Must(NotContainHtml)
            .WithMessage("Tiêu đề không được chứa HTML.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Nội dung tin nhắn không được để trống.")
            .MinimumLength(10)
            .WithMessage("Nội dung tin nhắn phải có ít nhất 10 ký tự.")
            .MaximumLength(5000)
            .WithMessage("Nội dung tin nhắn không được vượt quá 5000 ký tự.")
            .Must(NotContainHtml)
            .WithMessage("Nội dung tin nhắn không được chứa HTML.");

        RuleFor(x => x.Website)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Yêu cầu không hợp lệ.");
    }

    private static bool NotContainHtml(string value) =>
        !HtmlTagRegex().IsMatch(value);

    [GeneratedRegex(@"<\s*\/?\s*[a-zA-Z][^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();
}

