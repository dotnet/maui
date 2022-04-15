using System;
using System.Globalization;
using System.Threading.Tasks;
using Tizen.Applications;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
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

			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
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

			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}
	}
}
