using FluentValidation;
using Portfolio.Application.Resume.DTOs;

namespace Portfolio.Application.Resume.Validators;

public sealed class PublicCertificateFilterRequestValidator
    : AbstractValidator<PublicCertificateFilterRequest>
{
    public PublicCertificateFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .WithMessage("Từ khóa không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Organization)
            .MaximumLength(200)
            .WithMessage("Tên tổ chức không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));
    }
}

