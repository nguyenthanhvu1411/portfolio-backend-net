using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum AuditAction
    {
        [Display(Name = "Tạo mới")]
        Create = 1,

        [Display(Name = "Cập nhật")]
        Update = 2,

        [Display(Name = "Xóa")]
        Delete = 3,

        [Display(Name = "Đăng nhập")]
        Login = 4,

        [Display(Name = "Xuất bản")]
        Publish = 5
    }
}
