using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ShapesExtensions
	{
		public static void UpdatePath(this PlatformGraphicsView platformView, IShapeView shapeView)
		{
			var shapeDrawable = new ShapeDrawable(shapeView);

			var windingMode = shapeDrawable.GetPathWindingMode(shapeView);
			shapeDrawable.UpdateWindingMode(windingMode);

			platformView.Drawable = shapeDrawable;
		}
	}
}