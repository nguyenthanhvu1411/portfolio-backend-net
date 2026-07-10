using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum SkillLevel
    {
        [Display(Name = "Mới bắt đầu")]
        Beginner = 1,

        [Display(Name = "Trung bình")]
        Intermediate = 2,

        [Display(Name = "Nâng cao")]
        Advanced = 3
    }
}
