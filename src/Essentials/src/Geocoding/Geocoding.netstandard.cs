using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	class GeocodingImplementation : IGeocoding
	{
		public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
