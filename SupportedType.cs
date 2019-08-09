using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Csv2CSharpCli
{
    public enum SupportedType
    {
        [Display(Name = "string")]
        STRING,

        [Display(Name = "int")]
        INT,

        [Display(Name = "double")]
        DOUBLE,

        [Display(Name = "DateTime")]
        DATETIME,

        [Display(Name = "bool")]
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
