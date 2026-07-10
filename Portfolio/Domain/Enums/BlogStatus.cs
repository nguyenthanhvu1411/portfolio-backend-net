using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum BlogStatus
    {
        [Display(Name = "Bản nháp")]
        Draft = 1,

        [Display(Name = "Đã xuất bản")]
        Published = 2,

        [Display(Name = "Đã ẩn")]
        Hidden = 3
    }
}
