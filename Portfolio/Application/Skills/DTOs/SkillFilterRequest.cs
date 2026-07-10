namespace Portfolio.Application.Skills.DTOs;

public sealed class SkillFilterRequest
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
