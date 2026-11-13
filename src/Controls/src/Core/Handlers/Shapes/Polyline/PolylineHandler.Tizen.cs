#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler
	{
		protected override void ConnectHandler(MauiShapeView nativeView)
		{
			if (VirtualView is Polyline polyline)
			{
				UpdatePointsSubscription(polyline.Points);
			}

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiShapeView nativeView)
		{
			ClearPointsSubscription();

			base.DisconnectHandler(nativeView);
		}

		public static void MapShape(IShapeViewHandler handler, Polyline polyline)
		{
			handler.PlatformView?.UpdateShape(polyline);
		}

		public static void MapPoints(IShapeViewHandler handler, Polyline polyline)
		{
			if (handler is PolylineHandler polylineHandler)
			{
				polylineHandler.UpdatePointsSubscription(polyline.Points);
			}

			handler.PlatformView?.InvalidateShape(polyline);
		}

		public static void MapFillRule(IShapeViewHandler handler, Polyline polyline)
		{
			IDrawable drawable = handler.PlatformView?.Drawable;

			if (drawable == null)
				return;

			if (drawable is ShapeDrawable shapeDrawable)
				shapeDrawable.UpdateWindingMode(polyline.FillRule == FillRule.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero);

			handler.PlatformView?.InvalidateShape(polyline);
		}

	}
}