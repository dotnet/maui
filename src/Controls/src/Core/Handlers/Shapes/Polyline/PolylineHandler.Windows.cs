#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler
	{
		protected override void ConnectHandler(W2DGraphicsView nativeView)
		{
			if (VirtualView is Polyline polyline)
				polyline.Points.CollectionChanged += OnPointsCollectionChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(W2DGraphicsView nativeView)
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
				shapeDrawable.UpdateWindingMode(polyline.FillRule == FillRule.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero);

			handler.PlatformView?.InvalidateShape(polyline);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}