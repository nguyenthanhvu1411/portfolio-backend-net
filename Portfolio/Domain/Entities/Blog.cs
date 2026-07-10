using System;
using System.Collections.Generic;
using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities
{
    public class Blog : BaseEntity
    {
        public int CategoryId { get; set; }
        public BlogCategory Category { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }

        public BlogStatus Status { get; set; } = BlogStatus.Draft;

        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }

        public ICollection<BlogTagMapping> BlogTagMappings { get; set; } = new List<BlogTagMapping>();
        public ICollection<ViewStatistic> ViewStatistics { get; set; } = new List<ViewStatistic>();
    }
}
