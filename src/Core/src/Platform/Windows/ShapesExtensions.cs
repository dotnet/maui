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
			platformView.InvalidateShapeShadow();
		}

		public static void InvalidateShape(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			platformView.Invalidate();
			platformView.InvalidateShapeShadow();
		}

		// The shadow mask is a static snapshot of the shape, so refresh it whenever the shape changes.
		static void InvalidateShapeShadow(this W2DGraphicsView platformView)
		{
			if (platformView.Parent is WrapperView wrapperView)
			{
				wrapperView.InvalidateShadow();
			}
		}
	}
}
