using CoreGraphics;

namespace System.Graphics.CoreGraphics
{
    public static class CGColorExtensions
    {
        public static CGColor ToCGColor(this float[] color)
        {
            if (color == null)
                return null;

            return new CGColor(color[0], color[1], color[2], color[3]);
        }
    }
}