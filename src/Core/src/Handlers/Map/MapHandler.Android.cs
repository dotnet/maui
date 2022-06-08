using Android.Gms.Maps;
using Android.OS;
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
			//platformView.OnCreate(s_bundle);
			//platformView.OnResume();
		}

		protected override MapView CreatePlatformView()
		{
			_mapReady = new MapReadyCallbackHandler(this);
			MapView mapView = new Android.Gms.Maps.MapView(Context);

			//mapView.OnCreate(s_bundle);
			//mapView.OnResume();
			return mapView;
		}

		internal void OnMapReady(GoogleMap map)
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
