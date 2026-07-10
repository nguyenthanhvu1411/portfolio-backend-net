using FluentValidation;
using Portfolio.Application.Blogs.DTOs;

namespace Portfolio.Application.Blogs.Validators;

public sealed class BlogCategoryCreateRequestValidator : AbstractValidator<BlogCategoryCreateRequest>
{
    public BlogCategoryCreateRequestValidator() => ApplyRules(this);

    internal static void ApplyRules<T>(AbstractValidator<T> validator)
        where T : BlogCategoryCreateRequest
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .MaximumLength(100);
        validator.RuleFor(x => x.Slug).MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        validator.RuleFor(x => x.Description).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class BlogCategoryUpdateRequestValidator : AbstractValidator<BlogCategoryUpdateRequest>
{
    public BlogCategoryUpdateRequestValidator() => BlogCategoryCreateRequestValidator.ApplyRules(this);
}

public sealed class BlogFilterRequestValidator : AbstractValidator<BlogFilterRequest>
{
    public BlogFilterRequestValidator()
    {
        RuleFor(x => x.Keyword).MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class BlogCreateRequestValidator : AbstractValidator<BlogCreateRequest>
{
    public BlogCreateRequestValidator() => ApplyRules(this);

    internal static void ApplyRules<T>(AbstractValidator<T> validator)
        where T : BlogCreateRequest
    {
        validator.RuleFor(x => x.CategoryId).GreaterThan(0);
        validator.RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(250);
        validator.RuleFor(x => x.Slug).MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        validator.RuleFor(x => x.Summary).MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Summary));
        validator.RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung bài viết không được để trống.");
        validator.RuleForEach(x => x.Tags)
            .NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.Tags)
            .Must(x => x.Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() <= 20)
            .WithMessage("Mỗi bài viết chỉ được tối đa 20 tag.");
    }
}

public sealed class BlogUpdateRequestValidator : AbstractValidator<BlogUpdateRequest>
{
    public BlogUpdateRequestValidator() => BlogCreateRequestValidator.ApplyRules(this);
}
