using System.Globalization;
using System.Threading.Tasks;
using Tizen.Applications;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
		{
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

		public Task OpenMapsAsync(Placemark placemark, MapLaunchOptions options)
		{
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
