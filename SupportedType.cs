using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Csv2SqlCli
{
    public enum SupportedType
    {
        [Display(Name = "nvarchar")]
        STRING,

        [Display(Name = "int")]
        INT,

        [Display(Name = "float")]
        DOUBLE,

        [Display(Name = "datetime")]
        DATETIME,

        [Display(Name = "bit")]
        BOOL,


    }
    public static class SupportedTypeExtensions
    {
        public static string Name(this SupportedType supportedType)
        {
            return supportedType.GetType().GetMember(supportedType.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.Name ?? supportedType.ToString();
        }
    }
}
