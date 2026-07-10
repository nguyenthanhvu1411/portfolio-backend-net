namespace Portfolio.Application.Skills.DTOs;

public sealed class SkillReorderRequest
{
    public IReadOnlyList<SkillReorderItemRequest> Items { get; set; }
        = Array.Empty<SkillReorderItemRequest>();
}

public sealed class SkillReorderItemRequest
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}
