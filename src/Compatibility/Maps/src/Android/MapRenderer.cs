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
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using ACircle = Android.Gms.Maps.Model.Circle;
using APolygon = Android.Gms.Maps.Model.Polygon;
using APolyline = Android.Gms.Maps.Model.Polyline;
using Math = System.Math;
using Polygon = Microsoft.Maui.Controls.Maps.Polygon;
using Polyline = Microsoft.Maui.Controls.Maps.Polyline;
using Circle = Microsoft.Maui.Controls.Maps.Circle;

namespace Microsoft.Maui.Controls.Compatibility.Maps.Android
{
	public class MapRenderer : ViewRenderer<Map, MapView>, GoogleMap.IOnCameraMoveListener, IOnMapReadyCallback
	{
		const string MoveMessageName = "MapMoveToRegion";

		static Bundle s_bundle;

		bool _disposed;

		bool _init = true;

		List<Marker> _markers;
		List<APolyline> _polylines;
		List<APolygon> _polygons;
		List<ACircle> _circles;

		public MapRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		protected Map Map => Element;

		protected GoogleMap NativeMap;

		internal static Bundle Bundle
		{
			set { s_bundle = value; }
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			return new SizeRequest(new Size(Context.ToPixels(40), Context.ToPixels(40)));
		}

		protected override MapView CreateNativeControl()
		{
			return new MapView(Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);

					((ObservableCollection<Pin>)Element.Pins).CollectionChanged -= OnPinCollectionChanged;
					foreach (Pin pin in Element.Pins)
					{
						pin.PropertyChanged -= PinOnPropertyChanged;
					}

					((ObservableCollection<MapElement>)Element.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
					foreach (MapElement child in Element.MapElements)
					{
						child.PropertyChanged -= MapElementPropertyChanged;
					}
				}

				if (NativeMap != null)
				{
					NativeMap.MyLocationEnabled = false;
					NativeMap.TrafficEnabled = false;
					NativeMap.SetOnCameraMoveListener(null);
					NativeMap.MarkerClick -= OnMarkerClick;
					NativeMap.InfoWindowClick -= OnInfoWindowClick;
					NativeMap.MapClick -= OnMapClick;
					NativeMap.Dispose();
					NativeMap = null;
				}

				Control?.OnDestroy();
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			base.OnElementChanged(e);

			MapView oldMapView = Control;

			MapView mapView = CreateNativeControl();
			mapView.OnCreate(s_bundle);
			mapView.OnResume();
			SetNativeControl(mapView);

			if (e.OldElement != null)
			{
				Map oldMapModel = e.OldElement;

				((ObservableCollection<Pin>)oldMapModel.Pins).CollectionChanged -= OnPinCollectionChanged;
				foreach (Pin pin in oldMapModel.Pins)
				{
					pin.PropertyChanged -= PinOnPropertyChanged;
				}

				((ObservableCollection<MapElement>)oldMapModel.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
				foreach (MapElement child in oldMapModel.MapElements)
				{
					child.PropertyChanged -= MapElementPropertyChanged;
				}

				MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);

				if (NativeMap != null)
				{
					NativeMap.SetOnCameraMoveListener(null);
					NativeMap.MarkerClick -= OnMarkerClick;
					NativeMap.InfoWindowClick -= OnInfoWindowClick;
					NativeMap.MapClick -= OnMapClick;
					NativeMap = null;
				}

				oldMapView.Dispose();
			}

			Control.GetMapAsync(this);

			MessagingCenter.Subscribe<Map, MapSpan>(this, MoveMessageName, OnMoveToRegionMessage, Map);

			((INotifyCollectionChanged)Map.Pins).CollectionChanged += OnPinCollectionChanged;
			((INotifyCollectionChanged)Map.MapElements).CollectionChanged += OnMapElementCollectionChanged;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName)
			{
				SetMapType();
				return;
			}

			GoogleMap gmap = NativeMap;
			if (gmap == null)
			{
				return;
			}

			if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
			{
				SetUserVisible();
			}
			else if (e.PropertyName == Map.HasScrollEnabledProperty.PropertyName)
			{
				gmap.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
			}
			else if (e.PropertyName == Map.HasZoomEnabledProperty.PropertyName)
			{
				gmap.UiSettings.ZoomControlsEnabled = Map.HasZoomEnabled;
				gmap.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
			}
			else if (e.PropertyName == Map.TrafficEnabledProperty.PropertyName)
			{
				gmap.TrafficEnabled = Map.TrafficEnabled;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			if (_init)
			{
				if (NativeMap != null)
				{
					MoveToRegion(Element.LastMoveToRegion, false);
					OnPinCollectionChanged(Element.Pins, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					OnMapElementCollectionChanged(Element.MapElements, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					_init = false;
				}
			}
			else if (changed)
			{
				if (Element.MoveToLastRegionOnLayoutChange)
					MoveToRegion(Element.LastMoveToRegion, false);
			}

			if (NativeMap != null)
			{
				UpdateVisibleRegion(NativeMap.CameraPosition.Target);
			}

		}

		protected virtual void OnMapReady(GoogleMap map)
		{
			if (map == null)
			{
				return;
			}

			map.SetOnCameraMoveListener(this);
			map.MarkerClick += OnMarkerClick;
			map.InfoWindowClick += OnInfoWindowClick;
			map.MapClick += OnMapClick;

			map.TrafficEnabled = Map.TrafficEnabled;
			map.UiSettings.ZoomControlsEnabled = Map.HasZoomEnabled;
			map.UiSettings.ZoomGesturesEnabled = Map.HasZoomEnabled;
			map.UiSettings.ScrollGesturesEnabled = Map.HasScrollEnabled;
			SetUserVisible();
			SetMapType();
		}

		protected virtual MarkerOptions CreateMarker(Pin pin)
		{
			var opts = new MarkerOptions();
			opts.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
			opts.SetTitle(pin.Label);
			opts.SetSnippet(pin.Address);

			return opts;
		}

		void AddPins(IList pins)
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}

			if (_markers == null)
			{
				_markers = new List<Marker>();
			}

			_markers.AddRange(pins.Cast<Pin>().Select(p =>
			{
				Pin pin = p;
				var opts = CreateMarker(pin);
				var marker = map.AddMarker(opts);

				pin.PropertyChanged += PinOnPropertyChanged;

				// associate pin with marker for later lookup in event handlers
				pin.MarkerId = marker.Id;
				return marker;
			}));
		}

		void PinOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Pin pin = (Pin)sender;
			Marker marker = GetMarkerForPin(pin);

			if (marker == null)
			{
				return;
			}

			if (e.PropertyName == Pin.LabelProperty.PropertyName)
			{
				marker.Title = pin.Label;
			}
			else if (e.PropertyName == Pin.AddressProperty.PropertyName)
			{
				marker.Snippet = pin.Address;
			}
			else if (e.PropertyName == Pin.PositionProperty.PropertyName)
			{
				marker.Position = new LatLng(pin.Position.Latitude, pin.Position.Longitude);
			}
		}

		protected Marker GetMarkerForPin(Pin pin)
		{
			Marker targetMarker = null;

			if (_markers != null)
			{
				for (int i = 0; i < _markers.Count; i++)
				{
					var marker = _markers[i];
					if (marker.Id == (string)pin.MarkerId)
					{
						targetMarker = marker;
						break;
					}
				}
			}

			return targetMarker;
		}

		protected Pin GetPinForMarker(Marker marker)
		{
			Pin targetPin = null;

			for (int i = 0; i < Map.Pins.Count; i++)
			{
				var pin = Map.Pins[i];
				if ((string)pin.MarkerId == marker.Id)
				{
					targetPin = pin;
					break;
				}
			}

			return targetPin;
		}

		void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
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

		void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
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

		void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
		{
			Map.SendMapClicked(new Position(e.Point.Latitude, e.Point.Longitude));
		}

		void MoveToRegion(MapSpan span, bool animate)
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}

			span = span.ClampLatitude(85, -85);
			var ne = new LatLng(span.Center.Latitude + span.LatitudeDegrees / 2,
				span.Center.Longitude + span.LongitudeDegrees / 2);
			var sw = new LatLng(span.Center.Latitude - span.LatitudeDegrees / 2,
				span.Center.Longitude - span.LongitudeDegrees / 2);
			CameraUpdate update = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(sw, ne), 0);

			try
			{
				if (animate)
				{
					map.AnimateCamera(update);
				}
				else
				{
					map.MoveCamera(update);
				}
			}
			catch (IllegalStateException exc)
			{
				System.Diagnostics.Debug.WriteLine("MoveToRegion exception: " + exc);
				Log.Warning("Microsoft.Maui.Controls MapRenderer", $"MoveToRegion exception: {exc}");
			}
		}

		void OnPinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() => PinCollectionChanged(e));
			}
			else
			{
				PinCollectionChanged(e);
			}
		}

		void PinCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddPins(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemovePins(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					RemovePins(e.OldItems);
					AddPins(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Reset:
					if (_markers != null)
					{
						for (int i = 0; i < _markers.Count; i++)
							_markers[i].Remove();

						_markers = null;
					}

					AddPins((IList)Element.Pins);
					break;
				case NotifyCollectionChangedAction.Move:
					//do nothing
					break;
			}
		}

		void OnMoveToRegionMessage(Map s, MapSpan a)
		{
			MoveToRegion(a, true);
		}

		void RemovePins(IList pins)
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}
			if (_markers == null)
			{
				return;
			}

			foreach (Pin p in pins)
			{
				p.PropertyChanged -= PinOnPropertyChanged;
				var marker = GetMarkerForPin(p);

				if (marker == null)
				{
					continue;
				}
				marker.Remove();
				_markers.Remove(marker);
			}
		}

		void SetMapType()
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}

			switch (Map.MapType)
			{
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
					throw new ArgumentOutOfRangeException();
			}
		}

		void UpdateVisibleRegion(LatLng pos)
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}
			Projection projection = map.Projection;
			int width = Control.Width;
			int height = Control.Height;
			LatLng ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
			LatLng ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
			LatLng ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
			LatLng lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
			double dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
			double dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));
			Element.SetVisibleRegion(new MapSpan(new Position(pos.Latitude, pos.Longitude), dlat, dlong));
		}

		void MapElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (sender)
			{
				case Polyline polyline:
					{
						PolylineOnPropertyChanged(polyline, e);
						break;
					}
				case Polygon polygon:
					{
						PolygonOnPropertyChanged(polygon, e);
						break;
					}
				case Circle circle:
					{
						CircleOnPropertyChanged(circle, e);
						break;
					}
			}
		}

		void OnMapElementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() => MapElementCollectionChanged(e));
			}
			else
			{
				MapElementCollectionChanged(e);
			}
		}

		void MapElementCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddMapElements(e.NewItems.Cast<MapElement>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveMapElements(e.OldItems.Cast<MapElement>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveMapElements(e.OldItems.Cast<MapElement>());
					AddMapElements(e.NewItems.Cast<MapElement>());
					break;
				case NotifyCollectionChangedAction.Reset:
					if (_polylines != null)
					{
						for (int i = 0; i < _polylines.Count; i++)
							_polylines[i].Remove();

						_polylines = null;
					}

					if (_polygons != null)
					{
						for (int i = 0; i < _polygons.Count; i++)
							_polygons[i].Remove();

						_polygons = null;
					}

					if (_circles != null)
					{
						for (int i = 0; i < _circles.Count; i++)
							_circles[i].Remove();

						_circles = null;
					}

					AddMapElements(Element.MapElements);
					break;
			}
		}

		void AddMapElements(IEnumerable<MapElement> mapElements)
		{
			foreach (var element in mapElements)
			{
				element.PropertyChanged += MapElementPropertyChanged;

				switch (element)
				{
					case Polyline polyline:
						AddPolyline(polyline);
						break;
					case Polygon polygon:
						AddPolygon(polygon);
						break;
					case Circle circle:
						AddCircle(circle);
						break;
				}
			}
		}

		void RemoveMapElements(IEnumerable<MapElement> mapElements)
		{
			foreach (var element in mapElements)
			{
				element.PropertyChanged -= MapElementPropertyChanged;

				switch (element)
				{
					case Polyline polyline:
						RemovePolyline(polyline);
						break;
					case Polygon polygon:
						RemovePolygon(polygon);
						break;
					case Circle circle:
						RemoveCircle(circle);
						break;
				}
			}
		}

		#region Polylines

		protected virtual PolylineOptions CreatePolylineOptions(Polyline polyline)
		{
			var opts = new PolylineOptions();

			opts.InvokeColor(polyline.StrokeColor.ToAndroid(Color.Black));
			opts.InvokeWidth(polyline.StrokeWidth);

			foreach (var position in polyline.Geopath)
			{
				opts.Points.Add(new LatLng(position.Latitude, position.Longitude));
			}

			return opts;
		}

		protected APolyline GetNativePolyline(Polyline polyline)
		{
			APolyline targetPolyline = null;

			if (_polylines != null)
			{
				for (int i = 0; i < _polylines.Count; i++)
				{
					var native = _polylines[i];
					if (native.Id == (string)polyline.MapElementId)
					{
						targetPolyline = native;
						break;
					}
				}
			}

			return targetPolyline;
		}

		protected Polyline GetFormsPolyline(APolyline polyline)
		{
			Polyline targetPolyline = null;

			for (int i = 0; i < Map.MapElements.Count; i++)
			{
				var element = Map.MapElements[i];
				if ((string)element.MapElementId == polyline.Id)
				{
					targetPolyline = element as Polyline;
					break;
				}
			}

			return targetPolyline;
		}

		void PolylineOnPropertyChanged(Polyline formsPolyline, PropertyChangedEventArgs e)
		{
			var nativePolyline = GetNativePolyline(formsPolyline);

			if (nativePolyline == null)
			{
				return;
			}

			if (e.PropertyName == MapElement.StrokeColorProperty.PropertyName)
			{
				nativePolyline.Color = formsPolyline.StrokeColor.ToAndroid(Color.Black);
			}
			else if (e.PropertyName == MapElement.StrokeWidthProperty.PropertyName)
			{
				nativePolyline.Width = formsPolyline.StrokeWidth;
			}
			else if (e.PropertyName == nameof(Polyline.Geopath))
			{
				nativePolyline.Points = formsPolyline.Geopath.Select(position => new LatLng(position.Latitude, position.Longitude)).ToList();
			}
		}

		void AddPolyline(Polyline polyline)
		{
			var map = NativeMap;
			if (map == null)
			{
				return;
			}

			if (_polylines == null)
			{
				_polylines = new List<APolyline>();
			}

			var options = CreatePolylineOptions(polyline);
			var nativePolyline = map.AddPolyline(options);

			polyline.MapElementId = nativePolyline.Id;

			_polylines.Add(nativePolyline);
		}

		void RemovePolyline(Polyline polyline)
		{
			var native = GetNativePolyline(polyline);

			if (native != null)
			{
				native.Remove();
				_polylines.Remove(native);
			}
		}

		#endregion

		#region Polygons

		protected virtual PolygonOptions CreatePolygonOptions(Polygon polygon)
		{
			var opts = new PolygonOptions();

			opts.InvokeStrokeColor(polygon.StrokeColor.ToAndroid(Color.Black));
			opts.InvokeStrokeWidth(polygon.StrokeWidth);

			if (!polygon.StrokeColor.IsDefault)
				opts.InvokeFillColor(polygon.FillColor.ToAndroid());

			// Will throw an exception when added to the map if Points is empty
			if (polygon.Geopath.Count == 0)
			{
				opts.Points.Add(new LatLng(0, 0));
			}
			else
			{
				foreach (var position in polygon.Geopath)
				{
					opts.Points.Add(new LatLng(position.Latitude, position.Longitude));
				}
			}

			return opts;
		}

		protected APolygon GetNativePolygon(Polygon polygon)
		{
			APolygon targetPolygon = null;

			if (_polygons != null)
			{
				for (int i = 0; i < _polygons.Count; i++)
				{
					var native = _polygons[i];
					if (native.Id == (string)polygon.MapElementId)
					{
						targetPolygon = native;
						break;
					}
				}
			}

			return targetPolygon;
		}

		protected Polygon GetFormsPolygon(APolygon polygon)
		{
			Polygon targetPolygon = null;

			for (int i = 0; i < Element.MapElements.Count; i++)
			{
				var element = Element.MapElements[i];
				if ((string)element.MapElementId == polygon.Id)
				{
					targetPolygon = (Polygon)element;
					break;
				}
			}

			return targetPolygon;
		}

		void PolygonOnPropertyChanged(Polygon polygon, PropertyChangedEventArgs e)
		{
			var nativePolygon = GetNativePolygon(polygon);

			if (nativePolygon == null)
				return;

			if (e.PropertyName == MapElement.StrokeColorProperty.PropertyName)
			{
				nativePolygon.StrokeColor = polygon.StrokeColor.ToAndroid(Color.Black);
			}
			else if (e.PropertyName == MapElement.StrokeWidthProperty.PropertyName)
			{
				nativePolygon.StrokeWidth = polygon.StrokeWidth;
			}
			else if (e.PropertyName == Polygon.FillColorProperty.PropertyName)
			{
				nativePolygon.FillColor = polygon.FillColor.ToAndroid();
			}
			else if (e.PropertyName == nameof(polygon.Geopath))
			{
				nativePolygon.Points = polygon.Geopath.Select(p => new LatLng(p.Latitude, p.Longitude)).ToList();
			}
		}

		void AddPolygon(Polygon polygon)
		{
			var map = NativeMap;
			if (map == null)
			{
				return;
			}

			if (_polygons == null)
			{
				_polygons = new List<APolygon>();
			}

			var options = CreatePolygonOptions(polygon);
			var nativePolygon = map.AddPolygon(options);

			polygon.MapElementId = nativePolygon.Id;

			_polygons.Add(nativePolygon);
		}

		void RemovePolygon(Polygon polygon)
		{
			var native = GetNativePolygon(polygon);

			if (native != null)
			{
				native.Remove();
				_polygons.Remove(native);
			}
		}

		#endregion

		#region Circles

		protected virtual CircleOptions CreateCircleOptions(Circle circle)
		{
			var opts = new CircleOptions()
				.InvokeCenter(new LatLng(circle.Center.Latitude, circle.Center.Longitude))
				.InvokeRadius(circle.Radius.Meters)
				.InvokeStrokeWidth(circle.StrokeWidth);

			if (!circle.StrokeColor.IsDefault)
				opts.InvokeStrokeColor(circle.StrokeColor.ToAndroid());

			if (!circle.FillColor.IsDefault)
				opts.InvokeFillColor(circle.FillColor.ToAndroid());

			return opts;
		}

		protected ACircle GetNativeCircle(Circle circle)
		{
			ACircle targetCircle = null;

			if (_circles != null)
			{
				for (int i = 0; i < _circles.Count; i++)
				{
					var native = _circles[i];
					if (native.Id == (string)circle.MapElementId)
					{
						targetCircle = native;
						break;
					}
				}
			}

			return targetCircle;
		}

		protected Circle GetFormsCircle(ACircle circle)
		{
			Circle targetCircle = null;

			for (int i = 0; i < Element.MapElements.Count; i++)
			{
				var mapElement = Element.MapElements[i];
				if ((string)mapElement.MapElementId == circle.Id)
				{
					targetCircle = mapElement as Circle;
					break;
				}
			}

			return targetCircle;
		}

		void CircleOnPropertyChanged(Circle formsCircle, PropertyChangedEventArgs e)
		{
			var nativeCircle = GetNativeCircle(formsCircle);

			if (nativeCircle == null)
			{
				return;
			}

			if (e.PropertyName == Circle.FillColorProperty.PropertyName)
			{
				nativeCircle.FillColor = formsCircle.FillColor.ToAndroid();
			}
			else if (e.PropertyName == Circle.CenterProperty.PropertyName)
			{
				nativeCircle.Center = new LatLng(formsCircle.Center.Latitude, formsCircle.Center.Longitude);
			}
			else if (e.PropertyName == Circle.RadiusProperty.PropertyName)
			{
				nativeCircle.Radius = formsCircle.Radius.Meters;
			}
			else if (e.PropertyName == MapElement.StrokeColorProperty.PropertyName)
			{
				nativeCircle.StrokeColor = formsCircle.StrokeColor.ToAndroid();
			}
			else if (e.PropertyName == MapElement.StrokeWidthProperty.PropertyName)
			{
				nativeCircle.StrokeWidth = formsCircle.StrokeWidth;
			}
		}

		void AddCircle(Circle circle)
		{
			var map = NativeMap;
			if (map == null)
			{
				return;
			}

			if (_circles == null)
			{
				_circles = new List<ACircle>();
			}

			var options = CreateCircleOptions(circle);
			var nativeCircle = map.AddCircle(options);

			circle.MapElementId = nativeCircle.Id;

			_circles.Add(nativeCircle);
		}

		void RemoveCircle(Circle circle)
		{
			var native = GetNativeCircle(circle);

			if (native != null)
			{
				native.Remove();
				_circles.Remove(native);
			}
		}

		#endregion


		void SetUserVisible()
		{
			GoogleMap map = NativeMap;
			if (map == null)
			{
				return;
			}

			if (Map.IsShowingUser)
			{
				var coarseLocationPermission = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.AccessCoarseLocation);
				var fineLocationPermission = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.AccessFineLocation);

				if (coarseLocationPermission == Permission.Granted || fineLocationPermission == Permission.Granted)
				{
					map.MyLocationEnabled = map.UiSettings.MyLocationButtonEnabled = true;
				}
				else
				{
					Log.Warning("Microsoft.Maui.Controls.MapRenderer", "Missing location permissions for IsShowingUser");
					map.MyLocationEnabled = map.UiSettings.MyLocationButtonEnabled = false;
				}
			}
			else
			{
				map.MyLocationEnabled = map.UiSettings.MyLocationButtonEnabled = false;
			}
		}

		void IOnMapReadyCallback.OnMapReady(GoogleMap map)
		{
			NativeMap = map;
			OnMapReady(map);
		}

		void GoogleMap.IOnCameraMoveListener.OnCameraMove()
		{
			UpdateVisibleRegion(NativeMap.CameraPosition.Target);
		}
	}
}
