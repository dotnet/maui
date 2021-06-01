using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui
{
	public static class ShapesExtensions
	{
		public static void UpdateShape(this W2DGraphicsView nativeView, IShapeView shapeView)
		{
			nativeView.Drawable = new ShapeDrawable(shapeView);
		}

		public static void InvalidateShape(this W2DGraphicsView nativeView, IShapeView shapeView)
		{
			nativeView.Invalidate();
		}
	}
}
