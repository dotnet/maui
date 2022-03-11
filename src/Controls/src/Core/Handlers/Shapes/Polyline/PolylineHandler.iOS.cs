using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler
	{
		protected override void ConnectHandler(MauiShapeView nativeView)
		{
			if (VirtualView is Polyline polyline)
				polyline.Points.CollectionChanged += OnPointsCollectionChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiShapeView nativeView)
		{
			if (VirtualView is Polyline polyline)
				polyline.Points.CollectionChanged -= OnPointsCollectionChanged;

			base.DisconnectHandler(nativeView);
		}

		public static void MapShape(IShapeViewHandler handler, Polyline polyline)
		{
			handler.PlatformView?.UpdateShape(polyline);
		}

		public static void MapPoints(IShapeViewHandler handler, Polyline polyline)
		{
			handler.PlatformView?.InvalidateShape(polyline);
		}

		public static void MapFillRule(IShapeViewHandler handler, Polyline polyline)
		{
			IDrawable drawable = handler.PlatformView?.Drawable;

			if (drawable == null)
				return;

			if (drawable is ShapeDrawable shapeDrawable)
				shapeDrawable.WindingMode = polyline.FillRule == FillRule.EvenOdd ? Graphics.WindingMode.EvenOdd : Graphics.WindingMode.NonZero;

			handler.PlatformView?.InvalidateShape(polyline);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}