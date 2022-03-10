using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler
	{
		protected override void ConnectHandler(W2DGraphicsView nativeView)
		{
			if (VirtualView is Polygon polygon)
				polygon.Points.CollectionChanged += OnPointsCollectionChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(W2DGraphicsView nativeView)
		{
			if (VirtualView is Polygon polygon)
				polygon.Points.CollectionChanged -= OnPointsCollectionChanged;

			base.DisconnectHandler(nativeView);
		}

		public static void MapShape(PolygonHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

		public static void MapPoints(PolygonHandler handler, Polygon polygon)
		{
			handler.PlatformView?.InvalidateShape(polygon);
		}

		public static void MapFillRule(PolygonHandler handler, Polygon polygon)
		{
			IDrawable drawable = handler.PlatformView?.Drawable;

			if (drawable == null)
				return;

			if (drawable is ShapeDrawable shapeDrawable)
				shapeDrawable.WindingMode = polygon.FillRule == FillRule.EvenOdd ? Graphics.WindingMode.EvenOdd : Graphics.WindingMode.NonZero;

			handler.PlatformView?.InvalidateShape(polygon);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}