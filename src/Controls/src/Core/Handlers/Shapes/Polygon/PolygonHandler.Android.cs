using Microsoft.Maui.Controls.Shapes;

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

		public static void MapPoints(PolygonHandler handler, Polygon polygon) 
		{
			handler.PlatformView?.InvalidateShape(polygon);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}