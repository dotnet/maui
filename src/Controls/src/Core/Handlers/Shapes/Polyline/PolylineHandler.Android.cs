using Android.Content;
using Microsoft.Maui.Controls.Shapes;

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

		public static void MapPoints(PolylineHandler handler, Polyline polyline)
		{
			handler.PlatformView?.InvalidateShape(polyline);
		}

		void OnPointsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			PlatformView?.InvalidateShape(VirtualView);
		}
	}
}