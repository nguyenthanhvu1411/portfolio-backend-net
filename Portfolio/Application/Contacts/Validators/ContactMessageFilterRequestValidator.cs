using FluentValidation;
using Portfolio.Application.Contacts.DTOs;

namespace Portfolio.Application.Contacts.Validators;

public sealed class ContactMessageFilterRequestValidator
    : AbstractValidator<ContactMessageFilterRequest>
{
    public ContactMessageFilterRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(250)
            .WithMessage("Từ khóa không được vượt quá 250 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 100.");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("ToDate phải lớn hơn hoặc bằng FromDate.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
