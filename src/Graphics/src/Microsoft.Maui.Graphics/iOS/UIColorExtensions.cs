using System;
using UIKit;

namespace Microsoft.Maui.Graphics.Native
{
    public static class UIColorExtensions
    {
        public static string ToHex(this UIColor color)
        {
            if (color == null)
                return null;

            color.GetRGBA(out var red, out var green, out var blue, out var alpha);

            if (alpha < 1)
                return "#" + ToHexString(red) + ToHexString(green) + ToHexString(blue) + ToHexString(alpha);

            return "#" + ToHexString(red) + ToHexString(green) + ToHexString(blue);
        }

        private static string ToHexString(nfloat value)
        {
            var intValue = (int) (255f * value);
            var stringValue = intValue.ToString("X");
            if (stringValue.Length == 1)
                return "0" + stringValue;

            return stringValue;
        }
    }
}