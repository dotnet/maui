using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Devices.Sensors;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var uri = GetMapsUri(latitude, longitude, options);

			return OpenUri(uri);
		}

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var uri = GetMapsUri(placemark, options);

			return OpenUri(uri);
		}

		public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var uri = GetMapsUri(latitude, longitude, options);

			return TryOpenUri(uri);
		}

		public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var uri = GetMapsUri(placemark, options);

			return TryOpenUri(uri);
		}

		internal string GetMapsUri(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var uri = string.Empty;
			var lat = latitude.ToString(CultureInfo.InvariantCulture);
			var lng = longitude.ToString(CultureInfo.InvariantCulture);

			if (options.NavigationMode == NavigationMode.None)
			{
				uri = $"geo:{lat},{lng}?q={lat},{lng}";

				if (!string.IsNullOrWhiteSpace(options.Name))
					uri += $"({AndroidUri.Encode(options.Name)})";
			}
			else
			{
				uri = $"google.navigation:q={lat},{lng}{GetMode(options.NavigationMode)}";
			}

			return uri;
		}

		internal string GetMapsUri(Placemark placemark, MapLaunchOptions options)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var uri = string.Empty;
			if (options.NavigationMode == NavigationMode.None)
			{
				uri = $"geo:0,0?q={placemark.GetEscapedAddress()}";
				if (!string.IsNullOrWhiteSpace(options.Name))
					uri += $"({AndroidUri.Encode(options.Name)})";
			}
			else
			{
				uri = $"google.navigation:q={placemark.GetEscapedAddress()}{GetMode(options.NavigationMode)}";
			}

			return uri;
		}

		internal string GetMode(NavigationMode mode)
		{
			switch (mode)
			{
				case NavigationMode.Bicycling:
					return "&mode=b";
				case NavigationMode.Driving:
					return "&mode=d";
				case NavigationMode.Walking:
					return "&mode=w";
			}
			return string.Empty;
		}

		internal Task OpenUri(string uri)
		{
			var intent = ResolveMapIntent(uri);

			Platform.AppContext.StartActivity(intent);

			return Task.CompletedTask;
		}

		internal Task<bool> TryOpenUri(string uri)
		{
			var intent = ResolveMapIntent(uri);

			var canStart = PlatformUtils.IsIntentSupported(intent);

			if (canStart)
				Platform.AppContext.StartActivity(intent);

			return Task.FromResult(canStart);
		}

		Intent ResolveMapIntent(string uri)
		{
			var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri));
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			intent.SetFlags(flags);

			return intent;
		}
	}
}
