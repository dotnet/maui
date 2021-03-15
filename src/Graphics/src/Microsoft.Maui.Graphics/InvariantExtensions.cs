using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
    public static class InvariantExtensions
    {
        public static string ToInvariantString(this char target)
        {
            return target.ToString();
        }

        public static string ToInvariantString(this int target)
        {
            return target.ToString(CultureInfo.InvariantCulture);
        }
        
        public static bool EqualsIgnoresCase(this string target, string value)
        {
            return target.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}