using System;
using System.Collections.Generic;
using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? Role { get; set; }
        public string? ProjectType { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? DemoUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
        public ICollection<ProjectImage> ProjectImages { get; set; } = new List<ProjectImage>();
        public ICollection<ViewStatistic> ViewStatistics { get; set; } = new List<ViewStatistic>();
    }
}
