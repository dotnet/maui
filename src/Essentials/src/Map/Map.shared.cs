#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IMap
	{
		Task OpenAsync(double latitude, double longitude, MapLaunchOptions options);

		Task OpenAsync(Placemark placemark, MapLaunchOptions options);
	}

	public static class Map
	{
		static IMap? defaultImplementation;

		public static IMap Default =>
			defaultImplementation ??= new MapImplementation();

		internal static void SetDefault(IMap? implementation) =>
			defaultImplementation = implementation;
	}

	public static class MapExtensions
	{
		public static Task OpenAsync(this IMap map, Location location) =>
			map.OpenAsync(location, new MapLaunchOptions());

		public static Task OpenAsync(this IMap map, Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return map.OpenAsync(location.Latitude, location.Longitude, options);
		}

		public static Task OpenAsync(this IMap map, double latitude, double longitude) =>
			map.OpenAsync(latitude, longitude, new MapLaunchOptions());

		public static Task OpenAsync(this IMap map, Placemark placemark) =>
			map.OpenAsync(placemark, new MapLaunchOptions());
	}
}
