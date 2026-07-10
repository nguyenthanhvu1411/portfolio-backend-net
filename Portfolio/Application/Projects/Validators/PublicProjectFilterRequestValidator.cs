using FluentValidation;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Validators;

public sealed class PublicProjectFilterRequestValidator
    : AbstractValidator<PublicProjectFilterRequest>
{
    public PublicProjectFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .WithMessage("Từ khóa không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Technology)
            .MaximumLength(100)
            .WithMessage("Technology không được vượt quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Technology));

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái dự án không hợp lệ.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 50.");
    }
}

