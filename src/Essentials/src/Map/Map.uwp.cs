using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation:IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
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

			return LaunchUri(new Uri(uri));
		}

		static string GetMode(NavigationMode mode)
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

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
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

			return LaunchUri(new Uri(uri));
		}

		static Task LaunchUri(Uri mapsUri) =>
			global::Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();
	}
}
