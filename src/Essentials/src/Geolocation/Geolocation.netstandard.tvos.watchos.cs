using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		public Task<Location> GetLastKnownLocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
