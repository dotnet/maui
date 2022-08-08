using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using AndroidX.Core.Content;
using Java.Lang;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Math = System.Math;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, MapView>
	{
		bool _init = true;

		MapCallbackHandler? _mapReady;
		MapSpan? _lastMoveToRegion;
		List<Marker>? _markers;
		IList? _pins;
		//List<APolyline> _polylines;
		//List<APolygon> _polygons;
		//List<ACircle> _circles;

		public GoogleMap? Map { get; set; }

		static Bundle? s_bundle;

		public static Bundle? Bundle
		{
			set { s_bundle = value; }
		}

		protected override void ConnectHandler(MapView platformView)
		{
			base.ConnectHandler(platformView);
			_mapReady = new MapCallbackHandler(this);
			platformView.GetMapAsync(_mapReady);
			platformView.LayoutChange += MapViewLayoutChange;
			//(INotifyCollectionChanged)VirtualView.MapElements).CollectionChanged += OnMapElementCollectionChanged;
		}

		protected override void DisconnectHandler(MapView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.LayoutChange -= MapViewLayoutChange;
			
			if (Map != null)
			{
				Map.SetOnCameraMoveListener(null);
				Map.MarkerClick -= OnMarkerClick;
				Map.InfoWindowClick -= OnInfoWindowClick;
				Map.MapClick -= OnMapClick;
			}

			_mapReady = null;
		}

		protected override MapView CreatePlatformView()
		{
			MapView mapView = new MapView(Context);
			mapView.OnCreate(s_bundle);
			mapView.OnResume();
			return mapView;
		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.MapType = map.MapType switch
			{
				MapType.Street => GoogleMap.MapTypeNormal,
				MapType.Satellite => GoogleMap.MapTypeSatellite,
				MapType.Hybrid => GoogleMap.MapTypeHybrid,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			if (handler?.MauiContext?.Context == null)
				return;

			if (map.IsShowingUser)
			{
				var coarseLocationPermission = ContextCompat.CheckSelfPermission(handler.MauiContext.Context, Manifest.Permission.AccessCoarseLocation);
				var fineLocationPermission = ContextCompat.CheckSelfPermission(handler.MauiContext.Context, Manifest.Permission.AccessFineLocation);

				if (coarseLocationPermission == Permission.Granted || fineLocationPermission == Permission.Granted)
				{
					googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = true;
				}
				else
				{
					handler.MauiContext?.Services.GetService<ILogger<MapHandler>>()?.LogWarning("Missing location permissions for IsShowingUser");
					googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
				}
			}
			else
			{
				googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
			}
		}

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.UiSettings.ScrollGesturesEnabled = map.HasScrollEnabled;
		}

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.TrafficEnabled = map.HasTrafficEnabled;
		}

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.UiSettings.ZoomControlsEnabled = map.HasZoomEnabled;
			googleMap.UiSettings.ZoomGesturesEnabled = map.HasZoomEnabled;
		}

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		public static void MapPins(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler)
			{
				if (mapHandler._markers != null)
				{
					for (int i = 0; i < mapHandler._markers.Count; i++)
						mapHandler._markers[i].Remove();

					mapHandler._markers = null;
				}

				mapHandler.AddPins((IList)map.Pins);
			}
		}

		internal void OnMapReady(GoogleMap map)
		{
			if (map == null)
			{
				return;
			}

			Map = map;

			map.SetOnCameraMoveListener(_mapReady);
			map.MarkerClick += OnMarkerClick;
			map.InfoWindowClick += OnInfoWindowClick;
			map.MapClick += OnMapClick;
		}

		internal void UpdateVisibleRegion(LatLng pos)
		{
			if (Map == null)
				return;

			Projection projection = Map.Projection;
			int width = PlatformView.Width;
			int height = PlatformView.Height;
			LatLng ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
			LatLng ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
			LatLng ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
			LatLng lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
			double dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
			double dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));
			VirtualView.VisibleRegion = new MapSpan(new Devices.Sensors.Location(pos.Latitude, pos.Longitude), dlat, dlong);
		}

		void MapViewLayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs e)
		{
			if ((_init || VirtualView.MoveToLastRegionOnLayoutChange) && _lastMoveToRegion != null)
			{
				MoveToRegion(_lastMoveToRegion, false);
				if (_pins != null)
					AddPins(_pins);
				_init = false;
			}

			if (Map != null)
			{
				UpdateVisibleRegion(Map.CameraPosition.Target);
			}
		}

		void MoveToRegion(MapSpan span, bool animate)
		{
			_lastMoveToRegion = span;
			if (Map == null)
				return;

			var ne = new LatLng(span.Center.Latitude + span.LatitudeDegrees / 2,
				span.Center.Longitude + span.LongitudeDegrees / 2);
			var sw = new LatLng(span.Center.Latitude - span.LatitudeDegrees / 2,
				span.Center.Longitude - span.LongitudeDegrees / 2);
			CameraUpdate update = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(sw, ne), 0);

			try
			{
				if (animate)
				{
					Map.AnimateCamera(update);
				}
				else
				{
					Map.MoveCamera(update);
				}
			}
			catch (IllegalStateException exc)
			{
				MauiContext?.Services.GetService<ILogger<MapHandler>>()?.LogWarning(exc, $"MoveToRegion exception");
			}
		}

		void OnMarkerClick(object? sender, GoogleMap.MarkerClickEventArgs e)
		{
			var pin = GetPinForMarker(e.Marker);

			if (pin == null)
			{
				return;
			}

			// Setting e.Handled = true will prevent the info window from being presented
			// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
			bool handled = pin.SendMarkerClick();
			e.Handled = handled;
		}

		void OnInfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
		{
			var marker = e.Marker;
			var pin = GetPinForMarker(marker);

			if (pin == null)
			{
				return;
			}

			pin.SendMarkerClick();

			// SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
			bool hideInfoWindow = pin.SendInfoWindowClick();
			if (hideInfoWindow)
			{
				marker.HideInfoWindow();
			}
		}

		void OnMapClick(object? sender, GoogleMap.MapClickEventArgs e) =>
			VirtualView.Clicked(new Devices.Sensors.Location(e.Point.Latitude, e.Point.Longitude));

	
	
		void AddPins(IList pins)
		{
			//Mapper could be called before we have a Map ready 
			_pins = pins;
			if (Map == null || MauiContext == null)
				return;

			if (_markers == null)
				_markers = new List<Marker>();

			foreach (var p in pins)
			{
				IMapPin pin = (IMapPin)p;
				Marker marker;

				var pinHandler = pin.ToHandler(MauiContext);
				if (pinHandler is IMapPinHandler iMapPinHandler)
				{
					marker = Map.AddMarker(iMapPinHandler.PlatformView);
					// associate pin with marker for later lookup in event handlers
					pin.MarkerId = marker.Id;
					_markers.Add(marker);
				}

			}
			_pins = null;
		}

		protected IMapPin? GetPinForMarker(Marker marker)
		{
			IMapPin? targetPin = null;

			for (int i = 0; i < VirtualView.Pins.Count; i++)
			{
				var pin = VirtualView.Pins[i];
				if ((string)pin.MarkerId == marker.Id)
				{
					targetPin = pin;
					break;
				}
			}

			return targetPin;
		}
	}

	class MapCallbackHandler : Java.Lang.Object, GoogleMap.IOnCameraMoveListener, IOnMapReadyCallback
	{
		MapHandler? _handler;
		GoogleMap? _googleMap;

		public MapCallbackHandler(MapHandler mapHandler)
		{
			_handler = mapHandler;
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			_googleMap = googleMap;
			_handler?.OnMapReady(googleMap);
		}

		void GoogleMap.IOnCameraMoveListener.OnCameraMove()
		{
			if (_googleMap == null)
				return;

			_handler?.UpdateVisibleRegion(_googleMap.CameraPosition.Target);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _handler != null)
			{
				_handler = null;
				_googleMap = null;
			}

			base.Dispose(disposing);
		}
	}

}
