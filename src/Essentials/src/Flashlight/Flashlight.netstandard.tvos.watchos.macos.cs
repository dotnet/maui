using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Flashlight']/Docs" />
	public class FlashlightImplementation : IFlashlight
	{
		public Task TurnOnAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task TurnOffAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
