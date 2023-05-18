using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ShapesExtensions
	{
		public static void UpdatePath(this W2DGraphicsView platformView, IShapeView shapeView)
		{
			var shapeDrawable = new ShapeDrawable(shapeView);

			var windingMode = shapeDrawable.GetPathWindingMode(shapeView);
			shapeDrawable.UpdateWindingMode(windingMode);

			platformView.Drawable = shapeDrawable;
		}
	}
}