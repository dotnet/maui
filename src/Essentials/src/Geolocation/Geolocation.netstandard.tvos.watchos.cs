using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public partial class GeolocationImplementation : IGeolocation
	{
		public Task<Location> LastKnownLocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location> LocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location> LocationAsync(GeolocationRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location> LocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
