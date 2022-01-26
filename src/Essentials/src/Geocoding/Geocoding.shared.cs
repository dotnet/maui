using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geocoding']/Docs" />
	public static partial class Geocoding
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][0]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			return GetPlacemarksAsync(location.Latitude, location.Longitude);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetPlacemarksAsync'][1]/Docs" />
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
			=> PlatformGetPlacemarksAsync(latitude, longitude);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geocoding.xml" path="//Member[@MemberName='GetLocationsAsync']/Docs" />
		public static Task<IEnumerable<Location>> GetLocationsAsync(string address)
			=> PlatformGetLocationsAsync(address);
	}
}
