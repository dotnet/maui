using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public partial class GeolocationImplementation : IGeolocation
	{
		public Task<Location> GetLastKnownLocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
