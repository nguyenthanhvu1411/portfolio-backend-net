using System;

namespace Portfolio.Domain.Common
{
    /// <summary>
    /// Lớp cơ sở hỗ trợ xóa mềm.
    /// Chỉ kế thừa khi bảng có các cột IsDeleted, DeletedAt và DeletedBy.
    /// </summary>
    public abstract class BaseSoftDeleteEntity : BaseAuditableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
