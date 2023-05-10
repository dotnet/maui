using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Platform
{
	public static class ShapesExtensions
	{
		public static void UpdateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			platformView.Drawable = new ShapeDrawable(shapeView);
		}

		public static void InvalidateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			platformView.Invalidate();
		}
	}
}
