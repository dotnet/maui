// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	class GeocodingImplementation : IGeocoding
	{
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			using (var geocoder = new Geocoder(Application.Context))
			{
				var addressList = await geocoder.GetFromLocationAsync(latitude, longitude, 10);
				return addressList?.ToPlacemarks() ?? Array.Empty<Placemark>();
			}
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			using (var geocoder = new Geocoder(Application.Context))
			{
				var addressList = await geocoder.GetFromLocationNameAsync(address, 10);

				return addressList?.ToLocations() ?? Array.Empty<Location>();
			}
		}
	}
}
