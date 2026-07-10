using FluentValidation;
using Portfolio.Application.Users.DTOs;

namespace Portfolio.Application.Users.Validators;

public sealed class UserFilterRequestValidator : AbstractValidator<UserFilterRequest>
{
    public UserFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);
    }
}

public sealed class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.")
            .MaximumLength(255);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(200);

        RuleFor(x => x.Password).SetValidator(new StrongPasswordValidator());
        RuleFor(x => x.Status).IsInEnum();

        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(x => x.Count > 0).WithMessage("Tài khoản phải có ít nhất một vai trò.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("RoleIds không được trùng.");
    }
}

public sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.")
            .MaximumLength(255);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(200);

        RuleFor(x => x.AvatarUrl).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        RuleFor(x => x.Status).IsInEnum();

        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(x => x.Count > 0).WithMessage("Tài khoản phải có ít nhất một vai trò.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("RoleIds không được trùng.");
    }
}

public sealed class ResetUserPasswordRequestValidator : AbstractValidator<ResetUserPasswordRequest>
{
    public ResetUserPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword).SetValidator(new StrongPasswordValidator());
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống.")
            .Equal(x => x.NewPassword).WithMessage("Xác nhận mật khẩu không khớp.");
    }
}

internal sealed class StrongPasswordValidator : AbstractValidator<string>
{
    public StrongPasswordValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có chữ số.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải có ký tự đặc biệt.");
    }
}
