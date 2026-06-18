using System;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;

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

		// Note: ImageSource is handled in MauiMKMapView.GetViewForAnnotation
		// because the image is set on the MKAnnotationView, not on the IMKAnnotation
		public static void MapImageSource(IMapPinHandler handler, IMapPin mapPin)
		{
			// No-op: Image is applied when the annotation view is created in GetViewForAnnotation
		}
	}
}
