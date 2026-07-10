using System.Collections.Generic;
using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities
{
    public class Skill : BaseEntity
    {
        public int CategoryId { get; set; }
        public SkillCategory Category { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public SkillLevel? Level { get; set; }

        public string? IconUrl { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
    }
}
