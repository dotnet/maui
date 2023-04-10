using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ShapesExtensions
	{
		public static void UpdatePath(this PlatformGraphicsView platformView, IShapeView shapeView)
		{
			var shapeDrawable = new ShapeDrawable(shapeView);

			var windowMode = shapeDrawable.GetPathWindingMode(shapeView);
			shapeDrawable.UpdateWindingMode(windowMode);

			platformView.Drawable = shapeDrawable;
		}
	}
}