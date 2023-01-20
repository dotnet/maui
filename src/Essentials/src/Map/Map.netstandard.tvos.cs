using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	class MapImplementation : IMap
	{
		public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
