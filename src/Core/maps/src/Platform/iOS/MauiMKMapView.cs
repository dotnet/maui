using System;
using System.Collections;
using System.Linq;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Maps.Platform
{
	internal class MauiMKMapView : MKMapView
	{
		IMapHandler? _handler;
		object? _lastTouchedView;

		public MauiMKMapView(IMapHandler handler)
		{ 
			_handler = handler;
			OverlayRenderer = GetViewForOverlay1;
		}

		protected override void Dispose(bool disposing)
		{
			_handler = null;
			base.Dispose(disposing);
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (Window != null)
				Startup();
			else
				Cleanup();
		}

		void Startup()
		{
			RegionChanged += MkMapViewOnRegionChanged;
			DidSelectAnnotationView += MkMapViewOnAnnotationViewSelected;
		}

		void Cleanup()
		{
			if (_handler == null)
				return;

			RegionChanged -= MkMapViewOnRegionChanged;
			DidSelectAnnotationView -= MkMapViewOnAnnotationViewSelected;
			_handler = null;
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
					DeselectAnnotation(annotation, false);
				}
			}
		}

		void MkMapViewOnRegionChanged(object? sender, MKMapViewChangeEventArgs e)
		{
			if (_handler?.VirtualView != null)
				_handler.VirtualView.VisibleRegion = new MapSpan(new Devices.Sensors.Location(Region.Center.Latitude, Region.Center.Longitude), Region.Span.LatitudeDelta, Region.Span.LongitudeDelta);
		}

		IMapPin GetPinForAnnotation(IMKAnnotation annotation)
		{
			IMapPin targetPin = null!;
			IMap map = _handler?.VirtualView!;

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
#pragma warning disable CS8603 // Possible null reference return.
		protected virtual MKOverlayRenderer GetViewForOverlay1(MKMapView mapview, IMKOverlay overlay)
		{
			return overlay switch
			{
				MKPolyline polyline => GetViewForPolyline(polyline),
				MKPolygon polygon => GetViewForPolygon(polygon),
				MKCircle circle => GetViewForCircle(circle),
				_ => null,
			};
		}
#pragma warning restore CS8603 // Possible null reference return.
		protected virtual MKPolylineRenderer? GetViewForPolyline(MKPolyline mkPolyline)
		{
			var map = _handler?.VirtualView;
			IGeoPathMapElement? targetPolyline = null;

			for (int i = 0; i < map?.Elements.Count; i++)
			{
				var element = map.Elements[i];
				if (ReferenceEquals(element.MapElementId, mkPolyline))
				{
					targetPolyline = (IGeoPathMapElement)element;
					break;
				}
			}

			return targetPolyline?.ToHandler(_handler?.MauiContext!).PlatformView as MKPolylineRenderer;
		}

		protected virtual MKPolygonRenderer? GetViewForPolygon(MKPolygon mkPolygon)
		{
			var map = _handler?.VirtualView;
			IGeoPathMapElement? targetPolygon = null;

			for (int i = 0; i < map?.Elements.Count; i++)
			{
				var element = map.Elements[i];
				if (ReferenceEquals(element.MapElementId, mkPolygon))
				{
					targetPolygon = (IGeoPathMapElement)element;
					break;
				}
			}

			return targetPolygon?.ToHandler(_handler?.MauiContext!).PlatformView as MKPolygonRenderer;
		}

		protected virtual MKCircleRenderer? GetViewForCircle(MKCircle mkCircle)
		{
			var map = _handler?.VirtualView;
			ICircleMapElement? targetCircle = null;

			for (int i = 0; i < map?.Elements.Count; i++)
			{
				var element = map.Elements[i];
				if (ReferenceEquals(element.MapElementId, mkCircle))
				{
					targetCircle = (ICircleMapElement)element;
					break;
				}
			}

			return targetCircle?.ToHandler(_handler?.MauiContext!).PlatformView as MKCircleRenderer;
		}

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
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

		void OnCalloutClicked(IMKAnnotation annotation)
		{
			// lookup pin
			var targetPin = GetPinForAnnotation(annotation);

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
				DeselectAnnotation(annotation, true);
			}
		}

		internal void AddPins(IList pins)
		{
			if (_handler?.MauiContext == null)
				return;

			if (Annotations?.Length > 0)
				RemoveAnnotations(Annotations);

			foreach (IMapPin pin in pins)
			{
				if (pin.ToHandler(_handler.MauiContext).PlatformView is IMKAnnotation annotation)
				{
					pin.MarkerId = annotation;
					AddAnnotation(annotation);
				}
			}
		}

		internal void AddElements(IList elements)
		{
			foreach (IMapElement element in elements)
			{
				//	element.PropertyChanged += MapElementPropertyChanged;

				IMKOverlay? overlay = null;
				switch (element)
				{
					case IGeoPathMapElement geoPathElement:
						if (geoPathElement is IFilledMapElement)
							overlay = MKPolygon.FromCoordinates(geoPathElement.Geopath
							.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
							.ToArray());
						else
							overlay = MKPolyline.FromCoordinates(geoPathElement.Geopath
								.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
								.ToArray());
						break;
					case ICircleMapElement circleElement:
						overlay = MKCircle.Circle(
							new CLLocationCoordinate2D(circleElement.Center.Latitude, circleElement.Center.Longitude),
							circleElement.Radius.Meters);
						break;
				}

				if (overlay != null)
				{
					element.MapElementId = overlay;
					AddOverlay(overlay);
				}
			}
		}


	}
}
