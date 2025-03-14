#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		public Task<Location?> GetLastKnownLocationAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool IsListeningForeground { get => false; }

		public bool IsEnabled { get => false; }

		public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StopListeningForeground() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
