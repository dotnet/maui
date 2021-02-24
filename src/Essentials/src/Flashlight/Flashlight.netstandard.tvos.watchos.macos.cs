using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Flashlight
	{
		static Task PlatformTurnOnAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformTurnOffAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
