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
				polygon.Points.CollectionChanged += OnPointsCollectionChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiShapeView nativeView)
		{
			if (VirtualView is Polygon polygon)
				polygon.Points.CollectionChanged -= OnPointsCollectionChanged;

			base.DisconnectHandler(nativeView);
		}

		public static void MapShape(IShapeViewHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

		public static void MapPoints(IShapeViewHandler handler, Polygon polygon)
		{
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

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}