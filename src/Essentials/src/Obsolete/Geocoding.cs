#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geocoding']/Docs" />
	public static class Geocoding
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][1]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location) =>
			Current.GetPlacemarksAsync(location);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][2]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
			Current.GetPlacemarksAsync(latitude, longitude);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetLocationsAsync']/Docs" />
		public static Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
			Current.GetLocationsAsync(address);

		static IGeocoding Current => Devices.Sensors.Geocoding.Default;
	}
}
