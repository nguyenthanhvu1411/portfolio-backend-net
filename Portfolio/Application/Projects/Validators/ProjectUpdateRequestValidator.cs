using FluentValidation;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Validators;

public sealed class ProjectUpdateRequestValidator
    : AbstractValidator<ProjectUpdateRequest>
{
    public ProjectUpdateRequestValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Tên dự án không được để trống.")
            .MaximumLength(200).WithMessage("Tên dự án không được vượt quá 200 ký tự.");

        RuleFor(x => x.Slug)
            .MaximumLength(250).WithMessage("Slug không được vượt quá 250 ký tự.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug chỉ được chứa chữ thường, số và dấu gạch ngang.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.ShortDescription));

        RuleFor(x => x.FullDescription)
            .MaximumLength(20000)
            .When(x => !string.IsNullOrWhiteSpace(x.FullDescription));

        RuleFor(x => x.Role)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x.ProjectType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ProjectType));

        RuleFor(x => x.GithubUrl)
            .MaximumLength(500)
            .Must(BeValidHttpUrl)
            .WithMessage("GithubUrl phải là URL HTTP/HTTPS hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.GithubUrl));

        RuleFor(x => x.DemoUrl)
            .MaximumLength(500)
            .Must(BeValidHttpUrl)
            .WithMessage("DemoUrl phải là URL HTTP/HTTPS hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.DemoUrl));

        RuleFor(x => x.Status).IsInEnum();

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.SkillIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Danh sách kỹ năng chứa Id không hợp lệ.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Danh sách kỹ năng không được chứa Id trùng nhau.");
    }

    private static bool BeValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
