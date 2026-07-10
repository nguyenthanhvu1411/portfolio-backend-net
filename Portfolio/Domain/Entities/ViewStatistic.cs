using System;
using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities
{
    /// <summary>
    /// Bảng ViewStatistics có trong danh sách tổng quan nhưng file Excel chưa định nghĩa chi tiết cột.
    /// Các trường dưới đây là cấu trúc tối thiểu đề xuất để thống kê lượt xem website, blog và dự án.
    /// </summary>
    public class ViewStatistic : BaseEntity
    {
        public int? BlogId { get; set; }
        public Blog? Blog { get; set; }

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public string? PagePath { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
