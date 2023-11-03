using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml;

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

		public static void InvalidateMeasure(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			var containerView = platformView.Parent as FrameworkElement;

			bool invalidateMeasure = containerView is not null;

			if (invalidateMeasure)
				containerView?.InvalidateMeasure();
		}
	}
}
