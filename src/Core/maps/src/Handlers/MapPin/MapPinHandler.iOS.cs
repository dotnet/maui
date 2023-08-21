// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
	}
}
