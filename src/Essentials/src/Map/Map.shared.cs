using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Map']/Docs" />
	public static partial class Map
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][0]/Docs" />
		public static Task OpenAsync(Location location) =>
			OpenAsync(location, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task OpenAsync(Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task OpenAsync(double latitude, double longitude) =>
			OpenAsync(latitude, longitude, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return PlatformOpenMapsAsync(latitude, longitude, options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task OpenAsync(Placemark placemark) =>
			OpenAsync(placemark, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task OpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return PlatformOpenMapsAsync(placemark, options);
		}
	}
}
