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

namespace Microsoft.Maui.Maps.Handlers
{
	public class CustomMKMapView : MKMapView
	{
		internal event EventHandler<EventArgs>? LayoutSubviewsFired;
		public override void LayoutSubviews()
		{
			LayoutSubviewsFired?.Invoke(this, new EventArgs());
			base.LayoutSubviews();
		}
	}

	public partial class MapHandler : ViewHandler<IMap, MKMapView>
	{
		CLLocationManager? _locationManager;
		object? _lastTouchedView;
		bool _shouldUpdateRegion;

		UITapGestureRecognizer? _mapClickedGestureRecognizer;


		protected override MKMapView CreatePlatformView()
		{
			// See if we've got an MKMapView available in the pool; if so, use it
			var mapView = MapPool.Get() ?? new CustomMKMapView();

			return mapView;
		}

		protected override void ConnectHandler(MKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_locationManager = new CLLocationManager();

			_shouldUpdateRegion = true;
			PlatformView.AddGestureRecognizer(_mapClickedGestureRecognizer = new UITapGestureRecognizer(OnMapClicked));

			if(platformView is CustomMKMapView customMKMapView)
				customMKMapView.LayoutSubviewsFired += CustomMKMapViewLayoutSubviewsFired;

			PlatformView.DidSelectAnnotationView += MkMapViewOnAnnotationViewSelected;
			PlatformView.RegionChanged += MkMapViewOnRegionChanged;
			PlatformView.GetViewForAnnotation = GetViewForAnnotation;
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

			if (platformView is CustomMKMapView customMKMapView)
				customMKMapView.LayoutSubviewsFired -= CustomMKMapViewLayoutSubviewsFired;


			platformView.DidSelectAnnotationView -= MkMapViewOnAnnotationViewSelected;
			platformView.RegionChanged -= MkMapViewOnRegionChanged;
			platformView.GetViewForAnnotation = null;
			//platformView.OverlayRenderer = null;
		
			var mapsPinsItemsSource = (ObservableCollection<IMapPin>)VirtualView.Pins;
			mapsPinsItemsSource.CollectionChanged -= OnPinCollectionChanged;

			foreach (IMapPin pin in mapsPinsItemsSource)
			{
				pin.PropertyChanged -= PinOnPropertyChanged;
			}

			// This handler is done with the MKMapView; we can put it in the pool
			// for other rendererers to use in the future
			MapPool.Add(platformView);
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
			UpdateRegion();
		}

		void OnMapClicked(UITapGestureRecognizer recognizer)
		{
			var tapPoint = recognizer.LocationInView(PlatformView);
			var tapGPS = PlatformView.ConvertPoint(tapPoint, PlatformView);
			VirtualView.SendMapClick(new Devices.Sensors.Location(tapGPS.Latitude, tapGPS.Longitude));
		}

		void UpdateRegion()
		{
			if (_shouldUpdateRegion)
			{
				MoveToRegion(VirtualView.LastMoveToRegion, false);
				_shouldUpdateRegion = false;
			}
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

		MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView? mapPin;

			// https://bugzilla.xamarin.com/show_bug.cgi?id=26416
			var userLocationAnnotation = Runtime.GetNSObject(annotation.Handle) as MKUserLocation;
			if (userLocationAnnotation != null)
				return null!;

			const string defaultPinId = "defaultPin";
			mapPin = mapView.DequeueReusableAnnotation(defaultPinId);
			if (mapPin == null)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					mapPin = new MKMarkerAnnotationView(annotation, defaultPinId);
				}
				else
				{
					mapPin = new MKPinAnnotationView(annotation, defaultPinId);

				}
				
				mapPin.CanShowCallout = true;

				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					// Need to set this to get the callout bubble to show up
					// Without this no callout is shown, it's displayed differently
					mapPin.RightCalloutAccessoryView = new UIView();
				}
			}

			mapPin.Annotation = annotation;
			AttachGestureToPin(mapPin, annotation);

			return mapPin;
		}

		void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
		{
			var recognizers = mapPin.GestureRecognizers;

			if (recognizers != null)
			{
				foreach (var r in recognizers)
				{
					mapPin.RemoveGestureRecognizer(r);
				}
			}

			var recognizer = new UITapGestureRecognizer(g => OnCalloutClicked(annotation))
			{
				ShouldReceiveTouch = (gestureRecognizer, touch) =>
				{
					_lastTouchedView = touch.View;
					return true;
				}
			};

			mapPin.AddGestureRecognizer(recognizer);
		}

		IMapPin GetPinForAnnotation(IMKAnnotation annotation)
		{
			IMapPin targetPin = null!;
			var map = VirtualView;

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

		void OnCalloutClicked(IMKAnnotation annotation)
		{
			// lookup pin
			IMapPin targetPin = GetPinForAnnotation(annotation);

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
				PlatformView.DeselectAnnotation(annotation, true);
			}
		}

		void MkMapViewOnAnnotationViewSelected(object? sender, MKAnnotationViewEventArgs e)
		{
			var annotation = e.View.Annotation;
			var pin = GetPinForAnnotation(annotation!);

			if (pin != null)
			{
				// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
				// Hide the info window by deselecting the annotation
				bool deselect = pin.SendMarkerClick();
				if (deselect)
				{
					PlatformView.DeselectAnnotation(annotation, false);
				}
			}
		}

		void OnPinCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Dispatcher.GetForCurrentThread()?.Dispatch(() => PinCollectionChanged(e));
		}

		void PinCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Reset:
					if (PlatformView.Annotations?.Length > 0)
						PlatformView.RemoveAnnotations(PlatformView.Annotations);
					AddPins((IList)VirtualView.Pins);
					break;
				case NotifyCollectionChangedAction.Move:
					//do nothing
					break;
			}
		}

		void AddPins(IList pins)
		{
			foreach (IMapPin pin in pins)
			{
				pin.PropertyChanged += PinOnPropertyChanged;

				var annotation = CreateAnnotation(pin);
				pin.MarkerId = annotation;
				PlatformView.AddAnnotation(annotation);
			}
		}

		void RemovePins(IList pins)
		{
			foreach (IMapPin pin in pins)
			{
				pin.PropertyChanged -= PinOnPropertyChanged;
				PlatformView.RemoveAnnotation((IMKAnnotation)pin.MarkerId);
			}
		}

		IMKAnnotation CreateAnnotation(IMapPin pin)
		{
			return new MKPointAnnotation
			{
				Title = pin.Label,
				Subtitle = pin.Address ?? "",
				Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude),
			};
		}

		void PinOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			IMapPin pin = sender as IMapPin ?? throw new ArgumentNullException(nameof(sender), $"Argument cannot be null for {nameof(PinCollectionChanged)}");
			var annotation = pin.MarkerId as MKPointAnnotation;

			if (annotation == null)
			{
				return;
			}

			if (e.PropertyName == nameof(IMapPin.Label))
			{
				annotation.Title = pin.Label;
			}
			else if (e.PropertyName == nameof(IMapPin.Address))
			{
				annotation.Subtitle = pin.Address;
			}
			else if (e.PropertyName == nameof(IMapPin.Position))
			{
				annotation.Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude);
			}
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
	}
}
