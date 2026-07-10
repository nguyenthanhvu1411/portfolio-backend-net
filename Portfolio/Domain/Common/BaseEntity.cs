namespace Portfolio.Domain.Common
{
    /// <summary>
    /// Lớp cơ sở cho các entity có khóa chính đơn.
    /// Các bảng trung gian sử dụng khóa kép không kế thừa lớp này.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }
}
