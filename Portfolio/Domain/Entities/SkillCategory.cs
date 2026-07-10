using System.Collections.Generic;
using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class SkillCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
