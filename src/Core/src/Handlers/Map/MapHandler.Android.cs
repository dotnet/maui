using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using AndroidX.Core.Content;
using Java.Lang;
using Microsoft.Extensions.Logging;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ACircle = Android.Gms.Maps.Model.Circle;
using APolygon = Android.Gms.Maps.Model.Polygon;
using APolyline = Android.Gms.Maps.Model.Polyline;
//using Circle = Microsoft.Maui.Controls.Maps.Circle;
using Math = System.Math;
//using Polygon = Microsoft.Maui.Controls.Maps.Polygon;
//using Polyline = Microsoft.Maui.Controls.Maps.Polyline;

namespace Microsoft.Maui.Handlers
{
	class MapReady : Java.Lang.Object, IOnMapReadyCallback
	{
		//public IntPtr Handle => throw new NotImplementedException();

		public void OnMapReady(GoogleMap googleMap)
		{
			
		}
	}
	public partial class MapHandler : ViewHandler<IMap, Android.Gms.Maps.MapView>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		static Bundle? s_bundle;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static Bundle? Bundle
		{
			set { s_bundle = value; }
		}

		MapReady _mapReady = new MapReady();

		protected override void ConnectHandler(MapView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.GetMapAsync(_mapReady);
			//	platformView.OnCreate(s_bundle);
			//platformView.OnResume();
		}
		//GoogleMap NativeMap;
		protected override Android.Gms.Maps.MapView CreatePlatformView()
        {
			MapView mapView = new Android.Gms.Maps.MapView(Context);
			//mapView.OnCreate(s_bundle);
			//mapView.OnResume();
			return mapView;
        }

		//void IOnMapReadyCallback.OnMapReady(GoogleMap map)
		//{
		//	//NativeMap = map;
		//	OnMapReady(map);
		//}

		protected virtual void OnMapReady(GoogleMap map)
		{
			if (map == null)
			{
				return;
			}

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
