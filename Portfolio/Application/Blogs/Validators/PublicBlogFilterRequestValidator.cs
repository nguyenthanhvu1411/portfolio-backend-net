using FluentValidation;
using Portfolio.Application.Blogs.DTOs;

namespace Portfolio.Application.Blogs.Validators;

public sealed class PublicBlogFilterRequestValidator
    : AbstractValidator<PublicBlogFilterRequest>
{
    public PublicBlogFilterRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(250)
            .WithMessage("Từ khóa không được vượt quá 250 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId phải lớn hơn 0.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Tag)
            .MaximumLength(100)
            .WithMessage("Tag không được vượt quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Tag));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 50.");
    }
}

