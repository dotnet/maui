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

	/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Map']/Docs" />
	public static class Map
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task OpenAsync(Location location) =>
			Current.OpenAsync(location);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task OpenAsync(Location location, MapLaunchOptions options) =>
			Current.OpenAsync(location, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task OpenAsync(double latitude, double longitude) =>
			Current.OpenAsync(latitude, longitude);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		public static Task OpenAsync(double latitude, double longitude, MapLaunchOptions options) =>
			Current.OpenAsync(latitude, longitude, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task OpenAsync(Placemark placemark) =>
			Current.OpenAsync(placemark);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task OpenAsync(Placemark placemark, MapLaunchOptions options) =>
			Current.OpenAsync(placemark, options);

		static IMap Current => ApplicationModel.Map.Default;

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
