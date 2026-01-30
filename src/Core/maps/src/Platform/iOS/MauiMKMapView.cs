using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Maps.Platform
{
	public class MauiMKMapView : MKMapView
	{
		WeakReference<IMapHandler> _handlerRef;
		object? _lastTouchedView;
		UITapGestureRecognizer? _mapClickedGestureRecognizer;
		bool _isClusteringEnabled;

		public MauiMKMapView(IMapHandler handler)
		{
			_handlerRef = new WeakReference<IMapHandler>(handler);
			OverlayRenderer = GetViewForOverlayDelegate;
			// Assign custom annotation view delegate to enable gesture recognition on annotation callouts.
			base.GetViewForAnnotation = GetViewForAnnotation;
		}

		/// <summary>
		/// Gets or sets whether pin clustering is enabled.
		/// </summary>
		public bool IsClusteringEnabled
		{
			get => _isClusteringEnabled;
			set => _isClusteringEnabled = value;
		}

		internal IMapHandler? Handler
		{
			get
			{
				_handlerRef.TryGetTarget(out var handler);
				return handler;
			}
			set
			{
				if (value is not null)
				{
					_handlerRef = new WeakReference<IMapHandler>(value);
				}
			}
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (Window != null)
				Startup();
			else
				Cleanup();
		}

		protected virtual MKOverlayRenderer GetViewForOverlayDelegate(MKMapView mapview, IMKOverlay overlay)
		{
			MKOverlayRenderer? overlayRenderer = null;
			switch (overlay)
			{
				case MKPolyline polyline:
					overlayRenderer = GetMapElement<MKPolylineRenderer>(polyline);
					break;
				case MKPolygon polygon:
					overlayRenderer = GetMapElement<MKPolygonRenderer>(polygon);
					break;
				case MKCircle circle:
					overlayRenderer = GetMapElement<MKCircleRenderer>(circle);
					break;
				default:
					break;
			}
			if (overlayRenderer == null)
				throw new InvalidOperationException($"MKOverlayRenderer not found for {overlay}.");

			return overlayRenderer;
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

			// Handle cluster annotations
			if (annotation is MKClusterAnnotation clusterAnnotation)
			{
				return GetViewForClusterAnnotation(mapView, clusterAnnotation);
			}

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
			
			// Set clustering identifier if clustering is enabled
			if (_isClusteringEnabled && OperatingSystem.IsIOSVersionAtLeast(11))
			{
				// Get the clustering identifier from the pin
				var pin = GetPinForAnnotation(annotation);
				if (pin != null)
				{
					mapPin.ClusteringIdentifier = pin.ClusteringIdentifier;
				}
			}
			else if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				// Clear clustering identifier when disabled
				mapPin.ClusteringIdentifier = null;
			}
			
			AttachGestureToPin(mapPin, annotation);

			return mapPin;
		}

		MKAnnotationView GetViewForClusterAnnotation(MKMapView mapView, MKClusterAnnotation clusterAnnotation)
		{
			const string clusterId = "clusterPin";
			var clusterView = mapView.DequeueReusableAnnotation(clusterId) as MKMarkerAnnotationView;
			
			if (clusterView == null)
			{
				clusterView = new MKMarkerAnnotationView(clusterAnnotation, clusterId);
				clusterView.CanShowCallout = true;
			}
			
			clusterView.Annotation = clusterAnnotation;
			
			// Display the count of pins in the cluster
			var count = clusterAnnotation.MemberAnnotations?.Length ?? 0;
			clusterView.GlyphText = count.ToString();
			
			// Attach click handler for cluster
			AttachGestureToCluster(clusterView, clusterAnnotation);
			
			return clusterView;
		}

		void AttachGestureToCluster(MKAnnotationView clusterView, MKClusterAnnotation clusterAnnotation)
		{
			var recognizers = clusterView.GestureRecognizers;
			if (recognizers != null)
			{
				foreach (var recognizer in recognizers.OfType<UITapGestureRecognizer>().ToArray())
				{
					clusterView.RemoveGestureRecognizer(recognizer);
				}
			}

			var tapRecognizer = new UITapGestureRecognizer(() => OnClusterClicked(clusterAnnotation));
			clusterView.AddGestureRecognizer(tapRecognizer);
		}

		void OnClusterClicked(MKClusterAnnotation clusterAnnotation)
		{
			if (!_handlerRef.TryGetTarget(out var handler) || handler?.VirtualView == null)
				return;

			var memberAnnotations = clusterAnnotation.MemberAnnotations;
			if (memberAnnotations == null || memberAnnotations.Length == 0)
				return;

			// Convert member annotations to IMapPin list
			var pins = new List<IMapPin>();
			foreach (var memberAnnotation in memberAnnotations)
			{
				var pin = GetPinForAnnotation(memberAnnotation);
				if (pin != null)
				{
					pins.Add(pin);
				}
			}

			var coordinate = clusterAnnotation.Coordinate;
			var location = new Devices.Sensors.Location(coordinate.Latitude, coordinate.Longitude);

			// Call the handler's ClusterClicked method
			var handled = handler.VirtualView.ClusterClicked(pins, location);

			// If not handled, zoom to show all pins in the cluster
			if (!handled)
			{
				ZoomToShowClusterPins(memberAnnotations);
			}
		}

		void ZoomToShowClusterPins(IMKAnnotation[] annotations)
		{
			if (annotations.Length == 0)
				return;

			var rect = MKMapRect.Null;
			foreach (var annotation in annotations)
			{
				var point = MKMapPoint.FromCoordinate(annotation.Coordinate);
				rect = MKMapRect.Union(rect, new MKMapRect(point.X, point.Y, 0.1, 0.1));
			}

			// Add some padding
			var paddedRect = new MKMapRect(
				rect.MinX - rect.Width * 0.1,
				rect.MinY - rect.Height * 0.1,
				rect.Width * 1.2,
				rect.Height * 1.2);

			SetVisibleMapRect(paddedRect, true);
		}

		internal void AddPins(IList pins)
		{
			_handlerRef.TryGetTarget(out IMapHandler? handler);
			if (handler?.MauiContext == null)
				return;

			if (Annotations?.Length > 0)
				RemoveAnnotations(Annotations);

			foreach (IMapPin pin in pins)
			{
				if (pin.ToHandler(handler.MauiContext).PlatformView is IMKAnnotation annotation)
				{
					pin.MarkerId = annotation;
					AddAnnotation(annotation);
				}
			}
		}

		internal void ClearMapElements()
		{
			var elements = Overlays;

			if (elements == null)
				return;

			foreach (IMKOverlay overlay in elements)
			{
				RemoveOverlay(overlay);
			}
		}

		internal void AddElements(IList elements)
		{
			foreach (IMapElement element in elements)
			{
				IMKOverlay? overlay = null;
				switch (element)
				{
					case IGeoPathMapElement geoPathElement:
						if (geoPathElement is IFilledMapElement)
							overlay = MKPolygon.FromCoordinates(geoPathElement
							.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
							.ToArray());
						else
							overlay = MKPolyline.FromCoordinates(geoPathElement
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

		internal void RemoveElements(IList elements)
		{
			foreach (IMapElement element in elements)
			{
				if (element.MapElementId is IMKOverlay overlay)
					RemoveOverlay(overlay);
			}
		}

		void Startup()
		{
			RegionChanged += MkMapViewOnRegionChanged;
			DidSelectAnnotationView += MkMapViewOnAnnotationViewSelected;

			AddGestureRecognizer(_mapClickedGestureRecognizer = new UITapGestureRecognizer(OnMapClicked)
			{
				ShouldReceiveTouch = OnShouldReceiveMapTouch
			});
		}

		void Cleanup()
		{
			if (_mapClickedGestureRecognizer != null)
			{
				RemoveGestureRecognizer(_mapClickedGestureRecognizer);
				_mapClickedGestureRecognizer.Dispose();
				_mapClickedGestureRecognizer = null;
			}
			RegionChanged -= MkMapViewOnRegionChanged;
			DidSelectAnnotationView -= MkMapViewOnAnnotationViewSelected;
		}

		void MkMapViewOnAnnotationViewSelected(object? sender, MKAnnotationViewEventArgs e)
		{
			var annotation = e.View.Annotation;
			var pin = GetPinForAnnotation(annotation!);

			if (pin == null)
				return;

			// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
			// Hide the info window by deselecting the annotation
			bool deselect = pin.SendMarkerClick();

			if (deselect)
				DeselectAnnotation(annotation, false);
		}

		void MkMapViewOnRegionChanged(object? sender, MKMapViewChangeEventArgs e)
		{
			if (_handlerRef.TryGetTarget(out IMapHandler? handler) && handler?.VirtualView != null)
				handler.VirtualView.VisibleRegion = new MapSpan(new Devices.Sensors.Location(Region.Center.Latitude, Region.Center.Longitude), Region.Span.LatitudeDelta, Region.Span.LongitudeDelta);
		}

		IMapPin GetPinForAnnotation(IMKAnnotation annotation)
		{
			IMapPin targetPin = null!;
			_handlerRef.TryGetTarget(out IMapHandler? handler);
			IMap map = handler?.VirtualView!;

			for (int i = 0; i < map.Pins.Count; i++)
			{
				var pin = map.Pins[i];
				if ((pin?.MarkerId as IMKAnnotation) == annotation)
				{
					targetPin = pin;
					break;
				}
			}

			return targetPin;
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
				DeselectAnnotation(annotation, true);
		}

		T? GetMapElement<T>(IMKOverlay mkPolyline) where T : MKOverlayRenderer
		{
			_handlerRef.TryGetTarget(out IMapHandler? handler);
			var map = handler?.VirtualView;
			IMapElement mapElement = default!;
			for (int i = 0; i < map?.Elements.Count; i++)
			{
				var element = map.Elements[i];
				if (ReferenceEquals(element.MapElementId, mkPolyline))
				{
					mapElement = element;
					break;
				}
			}
			//Make sure we Disconnect old handler we don't want to reuse that one
			mapElement?.Handler?.DisconnectHandler();
			return mapElement?.ToHandler(handler?.MauiContext!).PlatformView as T;
		}

		static bool OnShouldReceiveMapTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (touch.View is MKAnnotationView)
				return false;

			return true;
		}

		static void OnMapClicked(UITapGestureRecognizer recognizer)
		{
			if (recognizer.View is not MauiMKMapView mauiMkMapView)
				return;

			var tapPoint = recognizer.LocationInView(mauiMkMapView);
			var tapGPS = mauiMkMapView.ConvertPoint(tapPoint, mauiMkMapView);

			if (mauiMkMapView._handlerRef.TryGetTarget(out IMapHandler? handler))
				handler?.VirtualView.Clicked(new Devices.Sensors.Location(tapGPS.Latitude, tapGPS.Longitude));
		}
	}
}
