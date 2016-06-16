using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Xamarin.Forms.Platform.iOS;
#if __UNIFIED__
using UIKit;
using MapKit;
using CoreLocation;
using Foundation;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using System.Drawing;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;
using ObjCRuntime;

#else
using nfloat=global::System.Single;
using nint=global::System.Int32;
using nuint=global::System.UInt32;
using MonoTouch.ObjCRuntime;
#endif

namespace Xamarin.Forms.Maps.iOS
{
	internal class MapDelegate : MKMapViewDelegate
	{
		// keep references alive, removing this will cause a segfault
		static readonly List<object> List = new List<object>();
		readonly Map _map;
		UIView _lastTouchedView;

		internal MapDelegate(Map map)
		{
			_map = map;
		}

#if __UNIFIED__
		public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
#else
		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
#endif
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

#if __UNIFIED__
		void AttachGestureToPin(MKPinAnnotationView mapPin, IMKAnnotation annotation)
#else
		void AttachGestureToPin (MKPinAnnotationView mapPin, NSObject annotation)
#endif
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

		void OnClick(object annotationObject, UITapGestureRecognizer recognizer)
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
	}

	public class MapRenderer : ViewRenderer
	{
	    CLLocationManager _locationManager;
		bool _shouldUpdateRegion;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					var mapModel = (Map)Element;
					MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
					((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
				}

				var mkMapView = (MKMapView)Control;
				mkMapView.RegionChanged -= MkMapViewOnRegionChanged;

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
				MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
			}

			if (e.NewElement != null)
			{
				var mapModel = (Map)e.NewElement;

				if (Control == null)
				{
					SetNativeControl(new MKMapView(RectangleF.Empty));
					var mkMapView = (MKMapView)Control;
					var mapDelegate = new MapDelegate(mapModel);
					mkMapView.GetViewForAnnotation = mapDelegate.GetViewForAnnotation;
					mkMapView.RegionChanged += MkMapViewOnRegionChanged;
				}

				MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) => MoveToRegion(a), mapModel);
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
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName && ((Map)Element).LastMoveToRegion != null)
				_shouldUpdateRegion = true;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
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
				var annotation = new MKPointAnnotation { Title = pin.Label, Subtitle = pin.Address ?? "" };

				pin.Id = annotation;
#if __UNIFIED__
				annotation.SetCoordinate(new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude));
#else
				annotation.Coordinate = new CLLocationCoordinate2D (pin.Position.Latitude, pin.Position.Longitude);
#endif
				((MKMapView)Control).AddAnnotation(annotation);
			}
		}

		void MkMapViewOnRegionChanged(object sender, MKMapViewChangeEventArgs mkMapViewChangeEventArgs)
		{
			if (Element == null)
				return;

			var mapModel = (Map)Element;
			var mkMapView = (MKMapView)Control;

			mapModel.VisibleRegion = new MapSpan(new Position(mkMapView.Region.Center.Latitude, mkMapView.Region.Center.Longitude), mkMapView.Region.Span.LatitudeDelta, mkMapView.Region.Span.LongitudeDelta);
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
#if __UNIFIED__
				((MKMapView)Control).RemoveAnnotation((IMKAnnotation)((Pin)pin).Id);
#else
				((MKMapView)Control).RemoveAnnotation ((NSObject)((Pin)pin).Id);
#endif
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
			if (FormsMaps.IsiOs8OrNewer && ((Map)Element).IsShowingUser)
			{
				_locationManager = new CLLocationManager();
				_locationManager.RequestWhenInUseAuthorization();
			}

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