using System;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, IMKAnnotation>
	{
		protected override IMKAnnotation CreatePlatformElement() => new MKPointAnnotation();

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (handler.PlatformView is MKPointAnnotation mKPointAnnotation)
				mKPointAnnotation.Coordinate = new CLLocationCoordinate2D(mapPin.Location.Latitude, mapPin.Location.Longitude);
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			if (handler.PlatformView is MKPointAnnotation mKPointAnnotation)
				mKPointAnnotation.Title = mapPin.Label;
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			if (handler.PlatformView is MKPointAnnotation mKPointAnnotation)
				mKPointAnnotation.Subtitle = mapPin.Address;
		}

		// The initial image is applied in MauiMKMapView.GetViewForAnnotation (set on the
		// MKAnnotationView, not the IMKAnnotation). This handles runtime ImageSource changes by
		// refreshing the annotation view of a pin that is already on the map.
		public static void MapImageSource(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.Parent?.Handler?.PlatformView is MauiMKMapView mapView)
				mapView.UpdatePinImage(mapPin);
		}
	}
}
