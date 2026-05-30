using System.Collections;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;

namespace Microsoft.Maui.Maps.Handlers
{

	public partial class MapHandler : ViewHandler<IMap, MauiMKMapView>
	{
		CLLocationManager? _locationManager;

		protected override MauiMKMapView CreatePlatformView()
		{
			return MapPool.Get(this) ?? new MauiMKMapView(this);
		}

		protected override void ConnectHandler(MauiMKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_locationManager = new CLLocationManager();
		}

		protected override void DisconnectHandler(MauiMKMapView platformView)
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

		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ScrollEnabled = map.IsScrollEnabled;
		}

		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ShowsTraffic = map.IsTrafficEnabled;
		}

		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ZoomEnabled = map.IsZoomEnabled;
		}

		public static void MapPins(IMapHandler handler, IMap map)
		{
			handler.PlatformView.AddPins((IList)map.Pins);
		}

		public static void MapElements(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ClearMapElements();
			handler.PlatformView.AddElements((IList)map.Elements);
		}

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		public void UpdateMapElement(IMapElement element)
		{
			PlatformView.RemoveElements(new[] { element });
			PlatformView.AddElements(new[] { element });
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			PlatformView.SetRegion(mapRegion, animated);
		}
	}
}
