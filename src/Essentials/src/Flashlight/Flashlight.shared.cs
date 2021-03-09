using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Flashlight
	{
		public static Task TurnOnAsync() =>
			PlatformTurnOnAsync();

		public static Task TurnOffAsync() =>
			PlatformTurnOffAsync();
	}
}
