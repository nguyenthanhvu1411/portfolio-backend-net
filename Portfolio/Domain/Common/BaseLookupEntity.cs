namespace Portfolio.Domain.Common
{
    /// <summary>
    /// Lớp cơ sở đề xuất cho các bảng danh mục dùng chung.
    /// Không áp dụng trực tiếp cho entity hiện tại nếu database chưa có đủ cột.
    /// </summary>
    public abstract class BaseLookupEntity : FullAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
