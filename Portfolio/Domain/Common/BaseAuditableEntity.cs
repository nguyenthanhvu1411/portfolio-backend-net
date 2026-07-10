using System;

namespace Portfolio.Domain.Common
{
    /// <summary>
    /// Lớp cơ sở cho entity cần lưu thông tin tạo và cập nhật.
    /// Chỉ sử dụng khi bảng dữ liệu có đủ các cột audit tương ứng.
    /// </summary>
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
