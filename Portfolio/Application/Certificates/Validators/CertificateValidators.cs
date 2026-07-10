using FluentValidation;
using Portfolio.Application.Certificates.DTOs;

namespace Portfolio.Application.Certificates.Validators;

public sealed class CertificateFilterRequestValidator : AbstractValidator<CertificateFilterRequest>
{
    public CertificateFilterRequestValidator()
    {
        RuleFor(x => x.Keyword).MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
        RuleFor(x => x.Organization).MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));
    }
}

public sealed class CertificateCreateRequestValidator : AbstractValidator<CertificateCreateRequest>
{
    public CertificateCreateRequestValidator() => ApplyRules(this);

    internal static void ApplyRules<T>(AbstractValidator<T> validator)
        where T : CertificateCreateRequest
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chứng chỉ không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.Organization)
            .NotEmpty().WithMessage("Tổ chức cấp không được để trống.")
            .MaximumLength(200);
        validator.RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Ngày hết hạn phải lớn hơn hoặc bằng ngày cấp.")
            .When(x => x.IssueDate.HasValue && x.ExpiryDate.HasValue);
        validator.RuleFor(x => x.CredentialId).MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CredentialId));
        validator.RuleFor(x => x.CredentialUrl)
            .MaximumLength(500)
            .Must(BeValidUrl).WithMessage("Credential URL không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.CredentialUrl));
        validator.RuleFor(x => x.Description).MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }

    private static bool BeValidUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public sealed class CertificateUpdateRequestValidator : AbstractValidator<CertificateUpdateRequest>
{
    public CertificateUpdateRequestValidator() => CertificateCreateRequestValidator.ApplyRules(this);
}
