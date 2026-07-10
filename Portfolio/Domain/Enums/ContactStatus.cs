using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum ContactStatus
    {
        [Display(Name = "Tin nhắn mới")]
        New = 1,

        [Display(Name = "Đã đọc")]
        Read = 2,

        [Display(Name = "Đã phản hồi")]
        Replied = 3,

        [Display(Name = "Đã lưu trữ")]
        Archived = 4
    }
}
