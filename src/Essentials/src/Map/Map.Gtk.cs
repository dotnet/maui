using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Map
	{
		internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
