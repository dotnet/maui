using System.Globalization;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials
{
	public static partial class Map
	{
		internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
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

		internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
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
