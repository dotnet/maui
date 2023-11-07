using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		public Task TurnOnAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task TurnOffAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
		
		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public Task<bool> IsSupportedAsync() => Task.FromResult(false);
	}
}
