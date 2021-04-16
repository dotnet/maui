using UIKit;

namespace Microsoft.Maui.Graphics.Native
{
    public static class UIKitExtensions
    {
        public static UIColor AsUIColor(this Color color)
        {
            if (color != null)
            {
                return UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);
            }

            return UIColor.White;
        }

        public static Color AsColor(this UIColor color)
        {
            if (color != null)
            {
                color.GetRGBA(out var red, out var green, out var blue, out var alpha);
                return new Color((float) red, (float) green, (float) blue, (float) alpha);
            }

            return new Color(1f, 1f, 1f, 1f); // White
        }


        public static UIBezierPath AsUIBezierPath(this PathF target)
        {
            if (target == null)
                return null;

            return UIBezierPath.FromPath(target.AsCGPath());
        }
    }
}