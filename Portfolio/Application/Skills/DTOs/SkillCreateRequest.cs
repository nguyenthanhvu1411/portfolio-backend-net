using Portfolio.Domain.Enums;

namespace Portfolio.Application.Skills.DTOs;

public sealed class SkillCreateRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillLevel? Level { get; set; }
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
}
