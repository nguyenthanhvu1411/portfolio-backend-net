using FluentValidation;
using Portfolio.Application.Profiles.DTOs;

namespace Portfolio.Application.Profiles.Validators;

public sealed class ProfileUpdateRequestValidator
    : AbstractValidator<ProfileUpdateRequest>
{
    public ProfileUpdateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Họ tên không được để trống.")
            .MinimumLength(2)
            .WithMessage("Họ tên phải có ít nhất 2 ký tự.")
            .MaximumLength(200)
            .WithMessage("Họ tên không được vượt quá 200 ký tự.");

        RuleFor(x => x.JobTitle)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Chức danh không được để trống.")
            .MinimumLength(2)
            .WithMessage("Chức danh phải có ít nhất 2 ký tự.")
            .MaximumLength(200)
            .WithMessage("Chức danh không được vượt quá 200 ký tự.");

        RuleFor(x => x.ShortBio)
            .MaximumLength(500)
            .WithMessage("Giới thiệu ngắn không được vượt quá 500 ký tự.");

        RuleFor(x => x.AboutMe)
            .MaximumLength(10_000)
            .WithMessage("Nội dung giới thiệu không được vượt quá 10.000 ký tự.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(255)
            .WithMessage("Email không được vượt quá 255 ký tự.")
            .EmailAddress()
            .WithMessage("Email không đúng định dạng.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithMessage("Số điện thoại không được vượt quá 50 ký tự.")
            .Matches(@"^\+?[0-9\s().-]{8,20}$")
            .WithMessage("Số điện thoại không đúng định dạng.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(255)
            .WithMessage("Địa chỉ không được vượt quá 255 ký tự.");

        RuleFor(x => x.GithubUrl)
            .Must(BeValidHttpUrl)
            .WithMessage("GitHub URL không hợp lệ.")
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.GithubUrl));

        RuleFor(x => x.LinkedinUrl)
            .Must(BeValidHttpUrl)
            .WithMessage("LinkedIn URL không hợp lệ.")
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedinUrl));

        RuleFor(x => x.FacebookUrl)
            .Must(BeValidHttpUrl)
            .WithMessage("Facebook URL không hợp lệ.")
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.FacebookUrl));
    }

    private static bool BeValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
