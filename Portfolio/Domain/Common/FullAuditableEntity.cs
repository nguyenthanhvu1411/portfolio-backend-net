using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Common
{
    /// <summary>
    /// Entity có audit, xóa mềm và kiểm soát cập nhật đồng thời.
    /// RowVersion được SQL Server sử dụng làm concurrency token.
    /// </summary>
    public abstract class FullAuditableEntity : BaseSoftDeleteEntity
    {
        [Timestamp]
        public uint Version { get; set; }
    }
}
