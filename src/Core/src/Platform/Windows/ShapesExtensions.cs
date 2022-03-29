using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Platform
{
	public static class ShapesExtensions
	{
		[System.Runtime.Versioning.SupportedOSPlatform("windows10.0.18362")]
		public static void UpdateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			platformView.Drawable = new ShapeDrawable(shapeView);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("windows10.0.18362")]
		public static void InvalidateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			platformView.Invalidate();
		}
	}
}
