using System.ComponentModel.DataAnnotations;

namespace Portfolio.Domain.Enums
{
    public enum UserStatus
    {
        [Display(Name = "Hoạt động")]
        Active = 1,

        [Display(Name = "Bị khóa")]
        Locked = 2,

        [Display(Name = "Ngưng hoạt động")]
        Inactive = 3
    }
}
