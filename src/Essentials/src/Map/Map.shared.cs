#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The Map API enables an application to open the installed map application to a specific location or placemark.
	/// </summary>
	public interface IMap
	{
		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task OpenAsync(double latitude, double longitude, MapLaunchOptions options);

		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task OpenAsync(Placemark placemark, MapLaunchOptions options);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific placemark with launch options.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options);
	}

	/// <summary>
	/// The Map API enables an application to open the installed map application to a specific location or placemark.
	/// </summary>
	public static class Map
	{
		/// <summary>
		/// Open the installed application to a specific location.
		/// </summary>
		/// <param name="location">Location to open in the map application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(Location location) =>
			Current.OpenAsync(location);

		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="location">Location to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(Location location, MapLaunchOptions options) =>
			Current.OpenAsync(location, options);

		/// <summary>
		/// Open the installed application to a specific location.
		/// </summary>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(double latitude, double longitude) =>
			Current.OpenAsync(latitude, longitude);

		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(double latitude, double longitude, MapLaunchOptions options) =>
			Current.OpenAsync(latitude, longitude, options);

		/// <summary>
		/// Open the installed application to a specific location.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(Placemark placemark) =>
			Current.OpenAsync(placemark);

		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(Placemark placemark, MapLaunchOptions options) =>
			Current.OpenAsync(placemark, options);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="location">Location to open in the map application.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(Location location) =>
			Current.TryOpenAsync(location);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="location">Location to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(Location location, MapLaunchOptions options) =>
			Current.TryOpenAsync(location, options);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(double latitude, double longitude) =>
			Current.TryOpenAsync(latitude, longitude);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="latitude">Latitude to open to.</param>
		/// <param name="longitude">Longitude to open to.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options) =>
			Current.TryOpenAsync(latitude, longitude, options);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then open the installed application to a specific placemark with launch options.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(Placemark placemark) =>
			Current.TryOpenAsync(placemark);

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then open the installed application to a specific placemark with launch options.
		/// </summary>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options) =>
			Current.TryOpenAsync(placemark, options);

		static IMap Current => ApplicationModel.Map.Default;

		static IMap? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IMap Default =>
			defaultImplementation ??= new MapImplementation();

		internal static void SetDefault(IMap? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IMap"/> APIs.
	/// </summary>
	public static class MapExtensions
	{
		/// <summary>
		/// Open the installed application to a specific location.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="location">Location to open in the map application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(this IMap map, Location location) =>
			map.OpenAsync(location, new MapLaunchOptions());

		/// <summary>
		/// Open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="location">Location to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown when either <paramref name="location"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
		public static Task OpenAsync(this IMap map, Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return map.OpenAsync(location.Latitude, location.Longitude, options);
		}

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="location">Location to open in the map application.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is <see langword="null"/>.</exception>
		public static Task<bool> TryOpenAsync(this IMap map, Location location) =>
			map.TryOpenAsync(location, new MapLaunchOptions());

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then open the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="location">Location to open in the map application.</param>
		/// <param name="options">Launch options to use.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when either <paramref name="location"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
		public static Task<bool> TryOpenAsync(this IMap map, Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return map.TryOpenAsync(location.Latitude, location.Longitude, options);
		}

		/// <summary>
		/// Open the installed application to a specific location.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(this IMap map, double latitude, double longitude) =>
			map.OpenAsync(latitude, longitude, new MapLaunchOptions());

		/// <summary>
		/// Open the installed application to a specific placemark.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OpenAsync(this IMap map, Placemark placemark) =>
			map.OpenAsync(placemark, new MapLaunchOptions());

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific location with launch options.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="latitude">Target latitude.</param>
		/// <param name="longitude">Target longitude.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(this IMap map, double latitude, double longitude) =>
			map.TryOpenAsync(latitude, longitude, new MapLaunchOptions());

		/// <summary>
		/// First checks if the installed map application can be opened,
		/// then opens the installed application to a specific placemark with launch options.
		/// </summary>
		/// <param name="map">The object this method is invoked on.</param>
		/// <param name="placemark">Placemark to open in the map application.</param>
		/// <returns><see langword="true"/> if the map application is opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> TryOpenAsync(this IMap map, Placemark placemark) =>
			map.TryOpenAsync(placemark, new MapLaunchOptions());
	}
}
