#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IGeocoding
	{
		Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude);
		Task<IEnumerable<Location>> GetLocationsAsync(string address);
	}
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geocoding']/Docs" />
	public static class Geocoding
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][1]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			return GetPlacemarksAsync(location.Latitude, location.Longitude);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][2]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
			=> Current.GetPlacemarksAsync(latitude, longitude);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetLocationsAsync']/Docs" />
		public static Task<IEnumerable<Location>> GetLocationsAsync(string address)
			=> Current.GetLocationsAsync(address);

		static IGeocoding? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IGeocoding Current =>
			currentImplementation ??= new GeocodingImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IGeocoding? implementation) =>
			currentImplementation = implementation;
	}
}
