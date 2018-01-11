using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreLocation;
using MapKit;
using ObjCRuntime;
using RectangleF = CoreGraphics.CGRect;
using Foundation;

#if __MOBILE__
using UIKit;
using Xamarin.Forms.Platform.iOS;
namespace Xamarin.Forms.Maps.iOS
#else
using AppKit;
using Xamarin.Forms.Platform.MacOS;
namespace Xamarin.Forms.Maps.MacOS
#endif
{
	internal class MapDelegate : MKMapViewDelegate
	{
		// keep references alive, removing this will cause a segfault
		readonly List<object> List = new List<object>();
		Map _map;
		object _lastTouchedView;
		bool _disposed;

		internal MapDelegate(Map map)
		{
			_map = map;
		}

		public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
		{
			MKPinAnnotationView mapPin = null;

			// https://bugzilla.xamarin.com/show_bug.cgi?id=26416
			var userLocationAnnotation = Runtime.GetNSObject(annotation.Handle) as MKUserLocation;
			if (userLocationAnnotation != null)
				return null;

			const string defaultPinId = "defaultPin";
			mapPin = (MKPinAnnotationView)mapView.DequeueReusableAnnotation(defaultPinId);
			if (mapPin == null)
			{
				mapPin = new MKPinAnnotationView(annotation, defaultPinId);
				mapPin.CanShowCallout = true;
			}

			mapPin.Annotation = annotation;
			AttachGestureToPin(mapPin, annotation);

			return mapPin;
		}
#if __MOBILE__
		void AttachGestureToPin(MKPinAnnotationView mapPin, IMKAnnotation annotation)
		{

			UIGestureRecognizer[] recognizers = mapPin.GestureRecognizers;

			if (recognizers != null)
			{
				foreach (UIGestureRecognizer r in recognizers)
				{
					mapPin.RemoveGestureRecognizer(r);
				}
			}

			Action<UITapGestureRecognizer> action = g => OnClick(annotation, g);
			var recognizer = new UITapGestureRecognizer(action) { ShouldReceiveTouch = (gestureRecognizer, touch) =>
			{
				_lastTouchedView = touch.View;
				return true;
			} };
			List.Add(action);
			List.Add(recognizer);
			mapPin.AddGestureRecognizer(recognizer);
			}
#else
		void AttachGestureToPin(MKPinAnnotationView mapPin, IMKAnnotation annotation)
		{
			NSGestureRecognizer[] recognizers = mapPin.GestureRecognizers;

			if (recognizers != null)
			{
				foreach (NSGestureRecognizer r in recognizers)
				{
					mapPin.RemoveGestureRecognizer(r);
				}
			}

			Action<NSClickGestureRecognizer> action = g => OnClick(annotation, g);
			var recognizer = new NSClickGestureRecognizer(action);
			List.Add(action);
			List.Add(recognizer);
			mapPin.AddGestureRecognizer(recognizer);

		}
#endif
#if __MOBILE__
		void OnClick(object annotationObject, UITapGestureRecognizer recognizer)
#else
		void OnClick(object annotationObject, NSClickGestureRecognizer recognizer)
#endif
		{
			// https://bugzilla.xamarin.com/show_bug.cgi?id=26416
			NSObject annotation = Runtime.GetNSObject(((IMKAnnotation)annotationObject).Handle);
			if (annotation == null)
				return;

			// lookup pin
			Pin targetPin = null;
			for (var i = 0; i < _map.Pins.Count; i++)
			{
				Pin pin = _map.Pins[i];
				object target = pin.Id;
				if (target != annotation)
					continue;

				targetPin = pin;
				break;
			}

			// pin not found. Must have been activated outside of forms
			if (targetPin == null)
				return;

			// if the tap happened on the annotation view itself, skip because this is what happens when the callout is showing
			// when the callout is already visible the tap comes in on a different view
			if (_lastTouchedView is MKAnnotationView)
				return;

			targetPin.SendTap();
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
				_map = null;
				_lastTouchedView = null;
			}

			base.Dispose(disposing);
		}
	}

	public class MapRenderer : ViewRenderer
	{
		CLLocationManager _locationManager;
		bool _shouldUpdateRegion;
		bool _disposed;

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
					((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
				}

				var mkMapView = (MKMapView)Control;
				mkMapView.RegionChanged -= MkMapViewOnRegionChanged;
				mkMapView.GetViewForAnnotation = null;
				if (mkMapView.Delegate != null)
				{
					mkMapView.Delegate.Dispose();
					mkMapView.Delegate = null;
				}
				mkMapView.RemoveFromSuperview();
#if __MOBILE__
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
				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
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

					var mkMapView = (MKMapView)Control;
					var mapDelegate = new MapDelegate(mapModel);
					mkMapView.GetViewForAnnotation = mapDelegate.GetViewForAnnotation;
					mkMapView.RegionChanged += MkMapViewOnRegionChanged;
				}

				MessagingCenter.Subscribe<Map, MapSpan>(this, MoveMessageName, (s, a) => MoveToRegion(a), mapModel);
				if (mapModel.LastMoveToRegion != null)
					MoveToRegion(mapModel.LastMoveToRegion, false);

				UpdateMapType();
				UpdateIsShowingUser();
				UpdateHasScrollEnabled();
				UpdateHasZoomEnabled();

				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged += OnCollectionChanged;

				OnCollectionChanged(((Map)Element).Pins, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
			else if (e.PropertyName == VisualElement.HeightProperty.PropertyName && ((Map)Element).LastMoveToRegion != null)
				_shouldUpdateRegion = true;
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
				var annotation = CreateAnnotation(pin);
				pin.Id = annotation;
				((MKMapView)Control).AddAnnotation(annotation);
			}
		}

		void MkMapViewOnRegionChanged(object sender, MKMapViewChangeEventArgs mkMapViewChangeEventArgs)
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

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			switch (notifyCollectionChangedEventArgs.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddPins(notifyCollectionChangedEventArgs.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemovePins(notifyCollectionChangedEventArgs.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					RemovePins(notifyCollectionChangedEventArgs.OldItems);
					AddPins(notifyCollectionChangedEventArgs.NewItems);
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
			foreach (object pin in pins)
				((MKMapView)Control).RemoveAnnotation((IMKAnnotation)((Pin)pin).Id);
		}

		void UpdateHasScrollEnabled()
		{
			((MKMapView)Control).ScrollEnabled = ((Map)Element).HasScrollEnabled;
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
	}
}
