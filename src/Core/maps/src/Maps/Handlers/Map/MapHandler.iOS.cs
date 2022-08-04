using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Dispatching;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Handlers;
using System.Drawing;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Maps.Handlers
{
	
	public partial class MapHandler : ViewHandler<IMap, MKMapView>
	{
		CLLocationManager? _locationManager;
		UITapGestureRecognizer? _mapClickedGestureRecognizer;

		protected override MKMapView CreatePlatformView()
		{
			return MapPool.Get() ?? new MauiMKMapView(this);
		}

		protected override void ConnectHandler(MKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_locationManager = new CLLocationManager();

			PlatformView.AddGestureRecognizer(_mapClickedGestureRecognizer = new UITapGestureRecognizer(OnMapClicked));

			if (platformView is MauiMKMapView customMKMapView)
				customMKMapView.LayoutSubviewsFired += CustomMKMapViewLayoutSubviewsFired;

			PlatformView.RegionChanged += MkMapViewOnRegionChanged;
			//PlatformView.OverlayRenderer = GetViewForOverlay;

			var mapsPinsItemsSource = (ObservableCollection<IMapPin>)VirtualView.Pins;
			mapsPinsItemsSource.CollectionChanged += OnPinCollectionChanged;
			OnPinCollectionChanged(mapsPinsItemsSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void DisconnectHandler(MKMapView platformView)
		{
			base.DisconnectHandler(platformView);

			if (_mapClickedGestureRecognizer != null)
			{
				PlatformView.RemoveGestureRecognizer(_mapClickedGestureRecognizer);
				_mapClickedGestureRecognizer.Dispose();
				_mapClickedGestureRecognizer = null;
			}

			if (platformView is MauiMKMapView customMKMapView)
				customMKMapView.LayoutSubviewsFired -= CustomMKMapViewLayoutSubviewsFired;


			platformView.RegionChanged -= MkMapViewOnRegionChanged;
			//platformView.OverlayRenderer = null;

			var mapsPinsItemsSource = (ObservableCollection<IMapPin>)VirtualView.Pins;
			mapsPinsItemsSource.CollectionChanged -= OnPinCollectionChanged;

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

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		//protected virtual MKOverlayRenderer GetViewForOverlay(MKMapView mapview, IMKOverlay overlay)
		//{
		//	//switch (overlay)
		//	//{
		//	//	case MKPolyline polyline:
		//	//		return GetViewForPolyline(polyline);
		//	//	case MKPolygon polygon:
		//	//		return GetViewForPolygon(polygon);
		//	//	case MKCircle circle:
		//	//		return GetViewForCircle(circle);
		//	//}

		//	return null;
		//}

		void CustomMKMapViewLayoutSubviewsFired(object? sender, EventArgs e)
		{
			//MoveToRegion(VirtualView.LastMoveToRegion, false);
		}

		void OnMapClicked(UITapGestureRecognizer recognizer)
		{
			var tapPoint = recognizer.LocationInView(PlatformView);
			var tapGPS = PlatformView.ConvertPoint(tapPoint, PlatformView);
			VirtualView.Clicked(new Devices.Sensors.Location(tapGPS.Latitude, tapGPS.Longitude));
		}

		void MkMapViewOnRegionChanged(object? sender, MKMapViewChangeEventArgs e)
		{
			VirtualView.VisibleRegion = new MapSpan(new Devices.Sensors.Location(PlatformView.Region.Center.Latitude, PlatformView.Region.Center.Longitude), PlatformView.Region.Span.LatitudeDelta, PlatformView.Region.Span.LongitudeDelta);
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			PlatformView.SetRegion(mapRegion, animated);
		}

		void OnPinCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Dispatcher.GetForCurrentThread()?.Dispatch(() => (PlatformView as MauiMKMapView)?.PinCollectionChanged(e));
		}
	}
}
