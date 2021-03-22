using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CoreLocation;
using MapKit;
using ObjCRuntime;
using RectangleF = CoreGraphics.CGRect;
using Microsoft.Maui.Controls.Maps;

#if __MOBILE__
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Maps.iOS
#else
using AppKit;
using Microsoft.Maui.Controls.Compatibility.Platform.MacOS;
namespace Microsoft.Maui.Controls.Compatibility.Maps.MacOS
#endif
{
	public class MapRenderer : ViewRenderer
	{
		CLLocationManager _locationManager;
		bool _shouldUpdateRegion;
		object _lastTouchedView;
		bool _disposed;

#if __MOBILE__
		UITapGestureRecognizer _mapClickedGestureRecognizer;
#endif

		const string MoveMessageName = "MapMoveToRegion";

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint);
		}

		// iOS 9/10 have some issues with releasing memory from map views; each one we create allocates
		// a bunch of memory we can never get back. Until that's fixed, we'll just reuse MKMapViews
		// as much as possible to prevent creating new ones and losing more memory

		// For the time being, we don't want ViewRenderer handling disposal of the MKMapView
		// if we're on iOS 9 or 10; during Dispose we'll be putting the MKMapView in a pool instead
#if __MOBILE__
		protected override bool ManageNativeControlLifetime => !FormsMaps.IsiOs9OrNewer;
#endif
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
					var mapModel = (Map)Element;
					MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);
					((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnPinCollectionChanged;
					((ObservableCollection<MapElement>)mapModel.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
					foreach (Pin pin in mapModel.Pins)
					{
						pin.PropertyChanged -= PinOnPropertyChanged;
					}
				}

				var mkMapView = (MKMapView)Control;
				mkMapView.DidSelectAnnotationView -= MkMapViewOnAnnotationViewSelected;
				mkMapView.RegionChanged -= MkMapViewOnRegionChanged;
				mkMapView.GetViewForAnnotation = null;
				mkMapView.OverlayRenderer = null;
				if (mkMapView.Delegate != null)
				{
					mkMapView.Delegate.Dispose();
					mkMapView.Delegate = null;
				}
				mkMapView.RemoveFromSuperview();
#if __MOBILE__
				mkMapView.RemoveGestureRecognizer(_mapClickedGestureRecognizer);
				_mapClickedGestureRecognizer.Dispose();
				_mapClickedGestureRecognizer = null;

				if (FormsMaps.IsiOs9OrNewer)
				{
					// This renderer is done with the MKMapView; we can put it in the pool
					// for other rendererers to use in the future
					MapPool.Add(mkMapView);
				}
#endif
				// For iOS versions < 9, the MKMapView will be disposed in ViewRenderer's Dispose method

				if (_locationManager != null)
				{
					_locationManager.Dispose();
					_locationManager = null;
				}

				_lastTouchedView = null;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var mapModel = (Map)e.OldElement;

				MessagingCenter.Unsubscribe<Map, MapSpan>(this, MoveMessageName);

				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnPinCollectionChanged;
				foreach (Pin pin in mapModel.Pins)
				{
					pin.PropertyChanged -= PinOnPropertyChanged;
				}

				((ObservableCollection<MapElement>)mapModel.MapElements).CollectionChanged -= OnMapElementCollectionChanged;
				foreach (MapElement mapElement in mapModel.MapElements)
				{
					mapElement.PropertyChanged -= MapElementPropertyChanged;
				}
			}

			if (e.NewElement != null)
			{
				var mapModel = (Map)e.NewElement;

				if (Control == null)
				{
					MKMapView mapView = null;
#if __MOBILE__
					if (FormsMaps.IsiOs9OrNewer)
					{
						// See if we've got an MKMapView available in the pool; if so, use it
						mapView = MapPool.Get();
					}
#endif
					if (mapView == null)
					{
						// If this is iOS 8 or lower, or if there weren't any MKMapViews in the pool,
						// create a new one
						mapView = new MKMapView(RectangleF.Empty);
					}

					SetNativeControl(mapView);

					mapView.GetViewForAnnotation = GetViewForAnnotation;
					mapView.OverlayRenderer = GetViewForOverlay;
					mapView.DidSelectAnnotationView += MkMapViewOnAnnotationViewSelected;
					mapView.RegionChanged += MkMapViewOnRegionChanged;
#if __MOBILE__
					mapView.AddGestureRecognizer(_mapClickedGestureRecognizer = new UITapGestureRecognizer(OnMapClicked));
#endif
				}

				MessagingCenter.Subscribe<Map, MapSpan>(this, MoveMessageName, (s, a) => MoveToRegion(a), mapModel);
				if (mapModel.LastMoveToRegion != null)
					MoveToRegion(mapModel.LastMoveToRegion, false);

				UpdateTrafficEnabled();
				UpdateMapType();
				UpdateIsShowingUser();
				UpdateHasScrollEnabled();
				UpdateHasZoomEnabled();

				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged += OnPinCollectionChanged;
				OnPinCollectionChanged(mapModel.Pins, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				((ObservableCollection<MapElement>)mapModel.MapElements).CollectionChanged += OnMapElementCollectionChanged;
				OnMapElementCollectionChanged(mapModel.MapElements, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName)
				UpdateMapType();
			else if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
				UpdateIsShowingUser();
			else if (e.PropertyName == Map.HasScrollEnabledProperty.PropertyName)
				UpdateHasScrollEnabled();
			else if (e.PropertyName == Map.HasZoomEnabledProperty.PropertyName)
				UpdateHasZoomEnabled();
			else if (e.PropertyName == Map.TrafficEnabledProperty.PropertyName)
				UpdateTrafficEnabled();
			else if (e.PropertyName == VisualElement.HeightProperty.PropertyName && ((Map)Element).LastMoveToRegion != null)
				_shouldUpdateRegion = ((Map)Element).MoveToLastRegionOnLayoutChange;
		}

#if __MOBILE__
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			UpdateRegion();
		}
#else
		public override void Layout()
		{
			base.Layout();
			UpdateRegion();
		}
#endif

		protected virtual IMKAnnotation CreateAnnotation(Pin pin)
		{
			return new MKPointAnnotation
			{
				Title = pin.Label,
				Subtitle = pin.Address ?? "",
				Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude)
			};
		}

		protected virtual MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView mapPin = null;

			// https://bugzilla.xamarin.com/show_bug.cgi?id=26416
			var userLocationAnnotation = Runtime.GetNSObject(annotation.Handle) as MKUserLocation;
			if (userLocationAnnotation != null)
				return null;

			const string defaultPinId = "defaultPin";
			mapPin = mapView.DequeueReusableAnnotation(defaultPinId);
			if (mapPin == null)
			{
				mapPin = new MKPinAnnotationView(annotation, defaultPinId);
				mapPin.CanShowCallout = true;
			}

			mapPin.Annotation = annotation;
			AttachGestureToPin(mapPin, annotation);

			return mapPin;
		}

		protected void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
		{
			var recognizers = mapPin.GestureRecognizers;

			if (recognizers != null)
			{
				foreach (var r in recognizers)
				{
					mapPin.RemoveGestureRecognizer(r);
				}
			}

#if __MOBILE__
			var recognizer = new UITapGestureRecognizer(g => OnCalloutClicked(annotation))
			{
				ShouldReceiveTouch = (gestureRecognizer, touch) =>
				{
					_lastTouchedView = touch.View;
					return true;
				}
			};
#else
			var recognizer = new NSClickGestureRecognizer(g => OnCalloutClicked(annotation));
#endif
			mapPin.AddGestureRecognizer(recognizer);
		}

		protected Pin GetPinForAnnotation(IMKAnnotation annotation)
		{
			Pin targetPin = null;
			var map = (Map)Element;

			for (int i = 0; i < map.Pins.Count; i++)
			{
				var pin = map.Pins[i];
				if ((IMKAnnotation)pin.MarkerId == annotation)
				{
					targetPin = pin;
					break;
				}
			}

			return targetPin;
		}

		void MkMapViewOnAnnotationViewSelected(object sender, MKAnnotationViewEventArgs e)
		{
			var annotation = e.View.Annotation;
			var pin = GetPinForAnnotation(annotation);

			if (pin != null)
			{
				// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
				// Hide the info window by deselecting the annotation
				bool deselect = pin.SendMarkerClick();
				if (deselect)
				{
					((MKMapView)Control).DeselectAnnotation(annotation, false);
				}
			}
		}

		void OnCalloutClicked(IMKAnnotation annotation)
		{
			// lookup pin
			Pin targetPin = GetPinForAnnotation(annotation);

			// pin not found. Must have been activated outside of forms
			if (targetPin == null)
				return;

			// if the tap happened on the annotation view itself, skip because this is what happens when the callout is showing
			// when the callout is already visible the tap comes in on a different view
			if (_lastTouchedView is MKAnnotationView)
				return;

			targetPin.SendMarkerClick();

			// SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
			// Hide the info window by deselecting the annotation
			bool deselect = targetPin.SendInfoWindowClick();
			if (deselect)
			{
				((MKMapView)Control).DeselectAnnotation(annotation, true);
			}
		}

#if __MOBILE__
		void OnMapClicked(UITapGestureRecognizer recognizer)
		{
			if (Element == null)
			{
				return;
			}

			var tapPoint = recognizer.LocationInView(Control);
			var tapGPS = ((MKMapView)Control).ConvertPoint(tapPoint, Control);
			((Map)Element).SendMapClicked(new Position(tapGPS.Latitude, tapGPS.Longitude));
		}
#endif

		void UpdateRegion()
		{
			if (_shouldUpdateRegion)
			{
				MoveToRegion(((Map)Element).LastMoveToRegion, false);
				_shouldUpdateRegion = false;
			}
		}

		void AddPins(IList pins)
		{
			foreach (Pin pin in pins)
			{
				pin.PropertyChanged += PinOnPropertyChanged;

				var annotation = CreateAnnotation(pin);
				pin.MarkerId = annotation;
				((MKMapView)Control).AddAnnotation(annotation);
			}
		}

		void PinOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Pin pin = (Pin)sender;
			var annotation = pin.MarkerId as MKPointAnnotation;

			if (annotation == null)
			{
				return;
			}

			if (e.PropertyName == Pin.LabelProperty.PropertyName)
			{
				annotation.Title = pin.Label;
			}
			else if (e.PropertyName == Pin.AddressProperty.PropertyName)
			{
				annotation.Subtitle = pin.Address;
			}
			else if (e.PropertyName == Pin.PositionProperty.PropertyName)
			{
				annotation.Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude);
			}

		}

		void MkMapViewOnRegionChanged(object sender, MKMapViewChangeEventArgs e)
		{
			if (Element == null)
				return;

			var mapModel = (Map)Element;
			var mkMapView = (MKMapView)Control;

			mapModel.SetVisibleRegion(new MapSpan(new Position(mkMapView.Region.Center.Latitude, mkMapView.Region.Center.Longitude), mkMapView.Region.Span.LatitudeDelta, mkMapView.Region.Span.LongitudeDelta));
		}

		void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			Position center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
			((MKMapView)Control).SetRegion(mapRegion, animated);
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
					var mapView = (MKMapView)Control;
					if (mapView.Annotations?.Length > 0)
						mapView.RemoveAnnotations(mapView.Annotations);
					AddPins((IList)(Element as Map).Pins);
					break;
				case NotifyCollectionChangedAction.Move:
					//do nothing
					break;
			}
		}

		void RemovePins(IList pins)
		{
			foreach (Pin pin in pins)
			{
				pin.PropertyChanged -= PinOnPropertyChanged;
				((MKMapView)Control).RemoveAnnotation((IMKAnnotation)pin.MarkerId);
			}
		}

		void UpdateHasScrollEnabled()
		{
			((MKMapView)Control).ScrollEnabled = ((Map)Element).HasScrollEnabled;
		}

		void UpdateTrafficEnabled()
		{
			((MKMapView)Control).ShowsTraffic = ((Map)Element).TrafficEnabled;
		}

		void UpdateHasZoomEnabled()
		{
			((MKMapView)Control).ZoomEnabled = ((Map)Element).HasZoomEnabled;
		}

		void UpdateIsShowingUser()
		{
#if __MOBILE__
			if (FormsMaps.IsiOs8OrNewer && ((Map)Element).IsShowingUser)
			{
				_locationManager = new CLLocationManager();
				_locationManager.RequestWhenInUseAuthorization();
			}
#endif
			((MKMapView)Control).ShowsUserLocation = ((Map)Element).IsShowingUser;
		}

		void UpdateMapType()
		{
			switch (((Map)Element).MapType)
			{
				case MapType.Street:
					((MKMapView)Control).MapType = MKMapType.Standard;
					break;
				case MapType.Satellite:
					((MKMapView)Control).MapType = MKMapType.Satellite;
					break;
				case MapType.Hybrid:
					((MKMapView)Control).MapType = MKMapType.Hybrid;
					break;
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
					var mkMapView = (MKMapView)Control;

					if (mkMapView.Overlays != null)
					{
						var overlays = mkMapView.Overlays;
						foreach (var overlay in overlays)
						{
							mkMapView.RemoveOverlay(overlay);
						}
					}

					AddMapElements(((Map)Element).MapElements);
					break;
			}
		}

		void AddMapElements(IEnumerable<MapElement> mapElements)
		{
			foreach (var element in mapElements)
			{
				element.PropertyChanged += MapElementPropertyChanged;

				IMKOverlay overlay = null;
				switch (element)
				{
					case Polyline polyline:
						overlay = MKPolyline.FromCoordinates(polyline.Geopath
							.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
							.ToArray());
						break;
					case Polygon polygon:
						overlay = MKPolygon.FromCoordinates(polygon.Geopath
							.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
							.ToArray());
						break;
					case Circle circle:
						overlay = MKCircle.Circle(
							new CLLocationCoordinate2D(circle.Center.Latitude, circle.Center.Longitude),
							circle.Radius.Meters);
						break;
				}

				element.MapElementId = overlay;

				((MKMapView)Control).AddOverlay(overlay);
			}
		}

		void RemoveMapElements(IEnumerable<MapElement> mapElements)
		{
			foreach (var element in mapElements)
			{
				element.PropertyChanged -= MapElementPropertyChanged;

				var overlay = (IMKOverlay)element.MapElementId;
				((MKMapView)Control).RemoveOverlay(overlay);
			}
		}

		void MapElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var element = (MapElement)sender;

			RemoveMapElements(new[] { element });
			AddMapElements(new[] { element });
		}

		protected virtual MKOverlayRenderer GetViewForOverlay(MKMapView mapview, IMKOverlay overlay)
		{
			switch (overlay)
			{
				case MKPolyline polyline:
					return GetViewForPolyline(polyline);
				case MKPolygon polygon:
					return GetViewForPolygon(polygon);
				case MKCircle circle:
					return GetViewForCircle(circle);
			}

			return null;
		}

		protected virtual MKPolylineRenderer GetViewForPolyline(MKPolyline mkPolyline)
		{
			var map = (Map)Element;
			Polyline targetPolyline = null;

			for (int i = 0; i < map.MapElements.Count; i++)
			{
				var element = map.MapElements[i];
				if (ReferenceEquals(element.MapElementId, mkPolyline))
				{
					targetPolyline = (Polyline)element;
					break;
				}
			}

			if (targetPolyline == null)
			{
				return null;
			}

			return new MKPolylineRenderer(mkPolyline)
			{
#if __MOBILE__
				StrokeColor = targetPolyline.StrokeColor.ToUIColor(Color.Black),
#else
				StrokeColor = targetPolyline.StrokeColor.ToNSColor(Color.Black),
#endif
				LineWidth = targetPolyline.StrokeWidth
			};
		}

		protected virtual MKPolygonRenderer GetViewForPolygon(MKPolygon mkPolygon)
		{
			var map = (Map)Element;
			Polygon targetPolygon = null;

			for (int i = 0; i < map.MapElements.Count; i++)
			{
				var element = map.MapElements[i];
				if (ReferenceEquals(element.MapElementId, mkPolygon))
				{
					targetPolygon = (Polygon)element;
					break;
				}
			}

			if (targetPolygon == null)
			{
				return null;
			}

			return new MKPolygonRenderer(mkPolygon)
			{
#if __MOBILE__
				StrokeColor = targetPolygon.StrokeColor.ToUIColor(Color.Black),
				FillColor = targetPolygon.FillColor.ToUIColor(),
#else
				StrokeColor = targetPolygon.StrokeColor.ToNSColor(Color.Black),
				FillColor = targetPolygon.FillColor.ToNSColor(),
#endif
				LineWidth = targetPolygon.StrokeWidth
			};
		}

		protected virtual MKCircleRenderer GetViewForCircle(MKCircle mkCircle)
		{
			var map = (Map)Element;
			Circle targetCircle = null;

			for (int i = 0; i < map.MapElements.Count; i++)
			{
				var element = map.MapElements[i];
				if (ReferenceEquals(element.MapElementId, mkCircle))
				{
					targetCircle = (Circle)element;
					break;
				}
			}

			if (targetCircle == null)
			{
				return null;
			}

			return new MKCircleRenderer(mkCircle)
			{
#if __MOBILE__
				StrokeColor = targetCircle.StrokeColor.ToUIColor(Color.Black),
				FillColor = targetCircle.FillColor.ToUIColor(),
#else
				StrokeColor = targetCircle.StrokeColor.ToNSColor(Color.Black),
				FillColor = targetCircle.FillColor.ToNSColor(),
#endif
				LineWidth = targetCircle.StrokeWidth
			};
		}
	}
}
