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
	}
}
