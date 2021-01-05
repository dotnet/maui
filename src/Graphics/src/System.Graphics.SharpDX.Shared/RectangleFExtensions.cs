using SharpDX.Mathematics.Interop;

namespace System.Graphics.SharpDX
{
    public static class RectangleFExtensions
    {
        public static RectangleF AsRectangleF(this RawRectangleF target)
        {
            var width = target.Right - target.Left;
            var height = target.Bottom - target.Top;
            return new RectangleF(target.Left, target.Top, width, height);
        }
    }
}