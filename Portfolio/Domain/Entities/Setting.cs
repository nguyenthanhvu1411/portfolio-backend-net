using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    public class Setting : BaseEntity
    {
        public string SiteName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? ThemeColor { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? ContactEmail { get; set; }
        public string? FooterText { get; set; }
    }
}
