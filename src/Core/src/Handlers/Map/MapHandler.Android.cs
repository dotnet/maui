using System;
using Android;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.OS;
using AndroidX.Core.Content;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	class MapReadyCallbackHandler : Java.Lang.Object, IOnMapReadyCallback
	{
		MapHandler _handler;
		public MapReadyCallbackHandler(MapHandler mapHandler)
		{
			_handler = mapHandler;
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			_handler.OnMapReady(googleMap);
		}
	}
	public partial class MapHandler : ViewHandler<IMap, MapView>
	{
		public GoogleMap? Map { get; set; }

		static Bundle? s_bundle;

		public static Bundle? Bundle
		{
			set { s_bundle = value; }
		}

		MapReadyCallbackHandler? _mapReady;

		protected override void ConnectHandler(MapView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.GetMapAsync(_mapReady);
		}

		protected override MapView CreatePlatformView()
		{
			_mapReady = new MapReadyCallbackHandler(this);
			MapView mapView = new Android.Gms.Maps.MapView(Context);
			mapView.OnCreate(s_bundle);
			mapView.OnResume();
			return mapView;
		}


		public static void MapMapType(IMapHander handler, IMap map)
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

		public static void MapIsShowingUser(IMapHander handler, IMap map)
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
					handler?.MauiContext?.CreateLogger<MapHandler>()?.LogWarning("Missing location permissions for IsShowingUser");
					googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
				}
			}
			else
			{
				googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
			}
		}

		public static void MapHasScrollEnabled(IMapHander handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.UiSettings.ScrollGesturesEnabled = map.HasScrollEnabled;
		}

		public static void MapHasTrafficEnabled(IMapHander handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.TrafficEnabled = map.HasTrafficEnabled;
		}

		public static void MapHasZoomEnabled(IMapHander handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			if (googleMap == null)
				return;

			googleMap.UiSettings.ZoomControlsEnabled = map.HasZoomEnabled;
			googleMap.UiSettings.ZoomGesturesEnabled = map.HasZoomEnabled;
		}

		internal void OnMapReady(GoogleMap map)
		{
			if (map == null)
			{
				return;
			}

			Map = map;

			//map.SetOnCameraMoveListener(this);
			//map.MarkerClick += OnMarkerClick;
			//map.InfoWindowClick += OnInfoWindowClick;
			//map.MapClick += OnMapClick;

			//map.TrafficEnabled = Map.TrafficEnabled;
			//map.UiSettings.ZoomControlsEnabled = Map.HasZoomEnabled;
			//map.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
			//map.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
			//SetUserVisible();
			//SetMapType();
		}

		//void GoogleMap.IOnCameraMoveListener.OnCameraMove()
		//{
		//	//UpdateVisibleRegion(NativeMap.CameraPosition.Target);
		//}
	}
}
