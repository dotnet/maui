using Microsoft.Maui.Controls.Shapes;
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

		public static void MapPoints(PolylineHandler handler, Polyline polyline)
		{
			handler.NativeView?.InvalidateShape(polyline);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NativeView?.InvalidateShape(VirtualView);
		}
	}
}