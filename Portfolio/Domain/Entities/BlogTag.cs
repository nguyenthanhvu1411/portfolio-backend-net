using System.Collections.Generic;
using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class BlogTag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public ICollection<BlogTagMapping> BlogTagMappings { get; set; } = new List<BlogTagMapping>();
    }
}
