using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Windows.System;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var uri = GetMapsUri(latitude, longitude, options);

			return LaunchUri(uri);
		}

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var uri = GetMapsUri(placemark, options);

			return LaunchUri(uri);
		}

		public async Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var uri = GetMapsUri(latitude, longitude, options);

			return await TryLaunchUri(uri);
		}

		public async Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var uri = GetMapsUri(placemark, options);

			return await TryLaunchUri(uri);
		}

		Uri GetMapsUri(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var lat = latitude.ToString(CultureInfo.InvariantCulture);
			var lng = longitude.ToString(CultureInfo.InvariantCulture);
			var name = options.Name ?? string.Empty;
			var uri = string.Empty;

			if (options.NavigationMode == NavigationMode.None)
			{
				uri = $"bingmaps:?collection=point.{lat}_{lng}_{name}";
			}
			else
			{
				uri = $"bingmaps:?rtp=~pos.{lat}_{lng}_{name}{GetMode(options.NavigationMode)}";
			}

			return new Uri(uri);
		}

		Uri GetMapsUri(Placemark placemark, MapLaunchOptions options)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var uri = string.Empty;

			if (options.NavigationMode == NavigationMode.None)
			{
				uri = $"bingmaps:?where={placemark.GetEscapedAddress()}";
			}
			else
			{
				uri = $"bingmaps:?rtp=~adr.{placemark.GetEscapedAddress()}{GetMode(options.NavigationMode)}";
			}

			return new Uri(uri);
		}

		string GetMode(NavigationMode mode)
		{
			switch (mode)
			{
				case NavigationMode.Driving:
					return "&mode=d";
				case NavigationMode.Transit:
					return "&mode=t";
				case NavigationMode.Walking:
					return "&mode=w";
			}
			return string.Empty;
		}

		async Task<bool> TryLaunchUri(Uri uri)
		{
			var canLaunch = await CanLaunchUri(uri);

			if (canLaunch)
			{
				await LaunchUri(uri);
			}

			return canLaunch;
		}

		Task LaunchUri(Uri mapsUri) =>
			global::Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();

		async Task<bool> CanLaunchUri(Uri uri)
		{
			var supported = await global::Windows.System.Launcher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
			return supported == LaunchQuerySupportStatus.Available;
		}
	}
}
