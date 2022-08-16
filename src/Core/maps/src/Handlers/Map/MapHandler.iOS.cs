using System.Collections;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;

namespace Microsoft.Maui.Maps.Handlers
{

	public partial class MapHandler : ViewHandler<IMap, MKMapView>
	{
		CLLocationManager? _locationManager;

		protected override MKMapView CreatePlatformView()
		{
			return MapPool.Get() ?? new MauiMKMapView(this);
		}

		protected override void ConnectHandler(MKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_locationManager = new CLLocationManager();
		}

		protected override void DisconnectHandler(MKMapView platformView)
		{
			base.DisconnectHandler(platformView);

			// This handler is done with the MKMapView; we can put it in the pool
			// for other rendererers to use in the future
			MapPool.Add(platformView);
		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{
			switch (map.MapType)
			{
				case MapType.Street:
					handler.PlatformView.MapType = MKMapType.Standard;
					break;
				case MapType.Satellite:
					handler.PlatformView.MapType = MKMapType.Satellite;
					break;
				case MapType.Hybrid:
					handler.PlatformView.MapType = MKMapType.Hybrid;
					break;
			}
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
#if !MACCATALYST
			if (map.IsShowingUser)
			{
				MapHandler? mapHandler = handler as MapHandler;
				mapHandler?._locationManager?.RequestWhenInUseAuthorization();
			}
#endif
			handler.PlatformView.ShowsUserLocation = map.IsShowingUser;
		}

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ScrollEnabled = map.HasScrollEnabled;
		}

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ShowsTraffic = map.HasTrafficEnabled;
		}

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ZoomEnabled = map.HasZoomEnabled;
		}

		public static void MapPins(IMapHandler handler, IMap map)
		{
			(handler.PlatformView as MauiMKMapView)?.AddPins((IList)map.Pins);
		}

		public static void MapElements(IMapHandler handler, IMap map)
		{
			(handler.PlatformView as MauiMKMapView)?.AddElements((IList)map.Elements);
		}

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			PlatformView.SetRegion(mapRegion, animated);
		}
	}
}
