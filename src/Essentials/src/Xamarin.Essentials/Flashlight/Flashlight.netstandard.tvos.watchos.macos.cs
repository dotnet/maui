using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class Flashlight
	{
		static Task PlatformTurnOnAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformTurnOffAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
