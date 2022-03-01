using SharpDX.Mathematics.Interop;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public static class RectFExtensions
	{
		public static RectF AsRectangleF(this RawRectangleF target)
		{
			var width = target.Right - target.Left;
			var height = target.Bottom - target.Top;
			return new RectF(target.Left, target.Top, width, height);
		}
	}
}
