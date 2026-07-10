using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class Education : BaseEntity
    {
        public string SchoolName { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? GPA { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
