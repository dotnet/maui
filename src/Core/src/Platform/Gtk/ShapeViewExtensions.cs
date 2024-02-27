using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{

	public static class ShapeViewExtensions
	{
		public static void UpdateShape(this MauiShapeView platformView, IShapeView shapeView)
		{
			platformView.Drawable = new ShapeDrawable(shapeView);
		}

		public static void InvalidateShape(this MauiShapeView platformView, IShapeView shapeView)
		{
			platformView.QueueDraw();
		}
	}

}