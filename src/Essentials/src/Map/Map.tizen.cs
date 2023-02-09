using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Tizen.Applications;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var appControl = GetAppControlData(latitude, longitude, options);
			return Launch(appControl);
		}

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var appControl = GetAppControlData(placemark, options);
			return Launch(appControl);
		}

		public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			var appControl = GetAppControlData(latitude, longitude, options);
			return TryLaunch(appControl);
		}

		public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			var appControl = GetAppControlData(placemark, options);
			return TryLaunch(appControl);
		}

		internal static AppControl GetAppControlData(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.View,
				Uri = "geo:",
			};

			appControl.Uri += $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";

			return appControl;
		}

		internal static AppControl GetAppControlData(Placemark placemark, MapLaunchOptions options)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.Pick,
				Uri = "geo:",
			};

			appControl.Uri += $"0,0?q={placemark.GetEscapedAddress()}";
			return appControl;
		}

		internal static Task Launch(AppControl appControl)
		{
			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}

		internal static Task<bool> TryLaunch(AppControl appControl)
		{
			var canLaunch = AppControl.GetMatchedApplicationIds(appControl).Any();

			if (canLaunch)
			{
				Launch(appControl);
			}

			return Task.FromResult(canLaunch);
		}
	}
}
