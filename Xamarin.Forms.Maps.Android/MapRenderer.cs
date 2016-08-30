using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Java.Lang;
using Xamarin.Forms.Platform.Android;
using Math = System.Math;
using Android.Runtime;
using System.Collections;

namespace Xamarin.Forms.Maps.Android
{
	public class MapRenderer : ViewRenderer<Map,MapView>, 
		GoogleMap.IOnCameraChangeListener 
	{
		public MapRenderer ()
		{
			AutoPackage = false;
		}

		static Bundle s_bundle;
		internal static Bundle Bundle { set { s_bundle = value; } }

		List<Marker> _markers;

		const string MoveMessageName = "MapMoveToRegion";

#pragma warning disable 618
		protected GoogleMap NativeMap => ((MapView) Control).Map;
#pragma warning restore 618

		protected Map Map => (Map) Element;

		public override SizeRequest GetDesiredSize (int widthConstraint, int heightConstraint)
		{
			return new SizeRequest (new Size (Context.ToPixels (40), Context.ToPixels (40)));
		}

		protected override MapView CreateNativeControl()
		{
			return new MapView(Context);
		}

		protected override void OnElementChanged (ElementChangedEventArgs<Map> e)
		{
			base.OnElementChanged (e);

			var oldMapView = (MapView)Control;

			var mapView = CreateNativeControl();
			mapView.OnCreate (s_bundle);
			mapView.OnResume ();
			SetNativeControl (mapView);

			if (e.OldElement != null) {
				var oldMapModel = e.OldElement;
				((ObservableCollection<Pin>)oldMapModel.Pins).CollectionChanged -= OnCollectionChanged;

				MessagingCenter.Unsubscribe<Map, MapSpan> (this, MoveMessageName);

#pragma warning disable 618
				if (oldMapView.Map != null) {
#pragma warning restore 618

#pragma warning disable 618
					oldMapView.Map.SetOnCameraChangeListener (null);
#pragma warning restore 618
					NativeMap.InfoWindowClick -= MapOnMarkerClick;
				}

				oldMapView.Dispose();
			}

			var map = NativeMap;
			if (map != null) {
				map.SetOnCameraChangeListener (this);
				NativeMap.InfoWindowClick += MapOnMarkerClick;

				map.UiSettings.ZoomControlsEnabled = Map.HasZoomEnabled;
				map.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
				map.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
				map.MyLocationEnabled = map.UiSettings.MyLocationButtonEnabled = Map.IsShowingUser;
				SetMapType ();
			}
			
			MessagingCenter.Subscribe<Map, MapSpan> (this, MoveMessageName, OnMoveToRegionMessage, Map);

			var incc = Map.Pins as INotifyCollectionChanged;
			if (incc != null)
				incc.CollectionChanged += OnCollectionChanged;
		}

		void OnCollectionChanged (object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			switch (notifyCollectionChangedEventArgs.Action) {
			case NotifyCollectionChangedAction.Add:
				AddPins (notifyCollectionChangedEventArgs.NewItems);
				break;
			case NotifyCollectionChangedAction.Remove:
				RemovePins (notifyCollectionChangedEventArgs.OldItems);
				break;
			case NotifyCollectionChangedAction.Replace:
				RemovePins (notifyCollectionChangedEventArgs.OldItems);
				AddPins (notifyCollectionChangedEventArgs.NewItems);
				break;
			case NotifyCollectionChangedAction.Reset:
					_markers?.ForEach (m => m.Remove ());
					_markers = null;
				AddPins ((IList)(Element as Map).Pins);
				break;
			case NotifyCollectionChangedAction.Move:
				//do nothing
				break;
			}
		}

		void OnMoveToRegionMessage (Map s, MapSpan a)
		{
			MoveToRegion (a, true);
		}

		void MoveToRegion (MapSpan span, bool animate)
		{
			var map = NativeMap;
			if (map == null)
				return;

			span = span.ClampLatitude (85, -85);
			var ne = new LatLng (span.Center.Latitude + span.LatitudeDegrees / 2, span.Center.Longitude + span.LongitudeDegrees / 2);
			var sw = new LatLng (span.Center.Latitude - span.LatitudeDegrees / 2, span.Center.Longitude - span.LongitudeDegrees / 2);
			var update = CameraUpdateFactory.NewLatLngBounds (new LatLngBounds (sw, ne), 0);
	
			try {
				if (animate)
					map.AnimateCamera (update);
				else
					map.MoveCamera (update);
			} catch (IllegalStateException exc) {
				System.Diagnostics.Debug.WriteLine ("MoveToRegion exception: " + exc);
			}
		}

		bool _init = true;

		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
			base.OnLayout (changed, l, t, r, b);

			if (_init) {
				MoveToRegion (((Map)Element).LastMoveToRegion, false);
				OnCollectionChanged (((Map)Element).Pins, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset));
				_init = false;
			} else if (changed) {
				UpdateVisibleRegion (NativeMap.CameraPosition.Target);
			}
		}

		protected override void OnElementPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged (sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName) {
				SetMapType();
				return;
			}
			
			var gmap = NativeMap;
			if (gmap == null)
				return;

			if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
				gmap.MyLocationEnabled = gmap.UiSettings.MyLocationButtonEnabled = Map.IsShowingUser;
			else if (e.PropertyName == Map.HasScrollEnabledProperty.PropertyName)
				gmap.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
			else if (e.PropertyName == Map.HasZoomEnabledProperty.PropertyName) {
				gmap.UiSettings.ZoomControlsEnabled = Map.HasZoomEnabled;
				gmap.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
			}
		}

		void SetMapType ()
		{
			var map = NativeMap;
			if (map == null)
				return;

			switch (Map.MapType) {
				case MapType.Street:
					map.MapType = GoogleMap.MapTypeNormal;
					break;
				case MapType.Satellite:
					map.MapType = GoogleMap.MapTypeSatellite;
					break;
				case MapType.Hybrid:
					map.MapType = GoogleMap.MapTypeHybrid;
					break;
				default:
					throw new ArgumentOutOfRangeException ();
			}
		}

		public void OnCameraChange (CameraPosition pos)
		{
			UpdateVisibleRegion (pos.Target);
		}

		void UpdateVisibleRegion (LatLng pos)
		{
			var map = NativeMap;
			if (map == null)
				return;
			var projection = map.Projection;
			var width = Control.Width;
			var height = Control.Height;
			var ul = projection.FromScreenLocation (new global::Android.Graphics.Point (0, 0));
			var ur = projection.FromScreenLocation (new global::Android.Graphics.Point (width, 0));
			var ll = projection.FromScreenLocation (new global::Android.Graphics.Point (0, height));
			var lr = projection.FromScreenLocation (new global::Android.Graphics.Point (width, height));
			var dlat = Math.Max (Math.Abs (ul.Latitude - lr.Latitude), Math.Abs (ur.Latitude - ll.Latitude));
			var dlong = Math.Max (Math.Abs (ul.Longitude - lr.Longitude), Math.Abs (ur.Longitude - ll.Longitude));
			((Map)Element).VisibleRegion = new MapSpan (
					new Position (
						pos.Latitude,
						pos.Longitude
					),
				dlat,
				dlong
			);
		}

		void AddPins (IList pins)
		{
			var map = NativeMap;
			if (map == null)
				return;

			if (_markers == null)
				_markers = new List<Marker> ();

			_markers.AddRange( pins.Cast<Pin>().Select (p => {
				var pin = (Pin)p;
				var opts = new MarkerOptions ();
				opts.SetPosition (new LatLng (pin.Position.Latitude, pin.Position.Longitude));
				opts.SetTitle (pin.Label);
				opts.SetSnippet (pin.Address);
				var marker = map.AddMarker (opts);

				// associate pin with marker for later lookup in event handlers
				pin.Id = marker.Id;
				return marker;
			}));
		}

		void RemovePins (IList pins)
		{
			var map = NativeMap;
			if (map == null)
				return;
			if (_markers == null)
				return;
			
			foreach (Pin p in pins) {
				var marker = _markers.FirstOrDefault (m => (object)m.Id == p.Id);
				if (marker == null)
					continue;
				marker.Remove ();
				_markers.Remove (marker);
			}
		}

		void MapOnMarkerClick (object sender, GoogleMap.InfoWindowClickEventArgs eventArgs)
		{
			// clicked marker
			var marker = eventArgs.Marker;

			// lookup pin
			Pin targetPin = null;
			for (var i = 0; i < Map.Pins.Count; i++) {
				var pin = Map.Pins[i];
				if ((string)pin.Id != marker.Id)
					continue;

				targetPin = pin;
				break;
			}

			// only consider event handled if a handler is present. 
			// Else allow default behavior of displaying an info window.
			targetPin?.SendTap ();
		}

		bool _disposed;
		protected override void Dispose (bool disposing)
		{
			if (disposing && !_disposed) {
				_disposed = true;
			
				var mapModel = Element as Map;
				if (mapModel != null) {
					MessagingCenter.Unsubscribe<Map, MapSpan> (this, MoveMessageName);
					((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
				}

				var gmap = NativeMap;
				if (gmap == null)
					return;
				gmap.MyLocationEnabled = false;
				gmap.InfoWindowClick -= MapOnMarkerClick;
				gmap.Dispose ();
			}

			base.Dispose (disposing);
		}
	}
}
