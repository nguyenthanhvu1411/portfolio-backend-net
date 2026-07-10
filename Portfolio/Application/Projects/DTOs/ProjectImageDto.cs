namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectImageDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsThumbnail { get; set; }
}
