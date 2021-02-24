using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Map
	{
		public static Task OpenAsync(Location location) =>
			OpenAsync(location, new MapLaunchOptions());

		public static Task OpenAsync(Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);
		}

		public static Task OpenAsync(double latitude, double longitude) =>
			OpenAsync(latitude, longitude, new MapLaunchOptions());

		public static Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return PlatformOpenMapsAsync(latitude, longitude, options);
		}

		public static Task OpenAsync(Placemark placemark) =>
			OpenAsync(placemark, new MapLaunchOptions());

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
