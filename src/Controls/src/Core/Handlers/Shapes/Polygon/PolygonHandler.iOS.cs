#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler
	{
		protected override void ConnectHandler(MauiShapeView nativeView)
		{
			if (VirtualView is Polygon polygon)
			{
				UpdatePointsSubscription(polygon.Points);
			}

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiShapeView nativeView)
		{
			ClearPointsSubscription();

			base.DisconnectHandler(nativeView);
		}

		public static void MapShape(IShapeViewHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

		public static void MapPoints(IShapeViewHandler handler, Polygon polygon)
		{
			if (handler is PolygonHandler polygonHandler)
			{
				polygonHandler.UpdatePointsSubscription(polygon.Points);
			}

			handler.PlatformView?.InvalidateShape(polygon);
		}

		public static void MapFillRule(IShapeViewHandler handler, Polygon polygon)
		{
			IDrawable drawable = handler.PlatformView?.Drawable;

			if (drawable == null)
				return;

			if (drawable is ShapeDrawable shapeDrawable)
				shapeDrawable.UpdateWindingMode(polygon.FillRule == FillRule.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero);

			handler.PlatformView?.InvalidateShape(polygon);
		}

	}
}