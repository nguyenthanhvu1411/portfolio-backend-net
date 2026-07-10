using System.Collections.Generic;
using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class BlogCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
