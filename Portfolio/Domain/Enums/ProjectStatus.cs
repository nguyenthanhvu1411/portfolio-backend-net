using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum ProjectStatus
    {
        [Display(Name = "Đang lên kế hoạch")]
        Planning = 1,

        [Display(Name = "Đang thực hiện")]
        InProgress = 2,

        [Display(Name = "Đã hoàn thành")]
        Completed = 3
    }
}
