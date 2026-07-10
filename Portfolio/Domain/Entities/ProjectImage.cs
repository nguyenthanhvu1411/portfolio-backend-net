using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class ProjectImage : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsThumbnail { get; set; }
    }
}
