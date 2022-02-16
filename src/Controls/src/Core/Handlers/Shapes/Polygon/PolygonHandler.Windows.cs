using Microsoft.Maui.Controls.Shapes;
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