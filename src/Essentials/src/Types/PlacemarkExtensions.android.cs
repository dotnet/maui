// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;

using AndroidAddress = Android.Locations.Address;

namespace Microsoft.Maui.Devices.Sensors
{
	static partial class PlacemarkExtensions
	{
		internal static IEnumerable<Placemark> ToPlacemarks(this IEnumerable<AndroidAddress> addresses)
		{
			return addresses.Select(address => new Placemark
			{
				Location = address.ToLocation(),
				FeatureName = address.FeatureName,
				PostalCode = address.PostalCode,
				SubLocality = address.SubLocality,
				CountryCode = address.CountryCode,
				CountryName = address.CountryName,
				Thoroughfare = address.Thoroughfare,
				SubThoroughfare = address.SubThoroughfare,
				Locality = address.Locality,
				AdminArea = address.AdminArea,
				SubAdminArea = address.SubAdminArea
			});
		}
	}
}
