using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Portfolio.Domain.Common
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Lấy tên hiển thị từ DisplayAttribute; nếu không có thì trả về tên enum.
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName()
                   ?? value.ToString();
        }
    }
}
