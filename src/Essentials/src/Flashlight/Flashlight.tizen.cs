using System.Threading.Tasks;
using Tizen.System;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class FlashlightImplementation:IFlashlight
	{
		internal static bool IsSupported
			=> Platform.GetFeatureInfo<bool>("camera.back.flash");

		public Task TurnOnAsync()
		{
			return SwitchFlashlight(true);
		}

		public Task TurnOffAsync()
		{
			return SwitchFlashlight(false);
		}

		Task SwitchFlashlight(bool switchOn)
		{
			Permissions.EnsureDeclared<Permissions.Flashlight>();
			return Task.Run(() =>
			{
				if (!IsSupported)
					throw new FeatureNotSupportedException();

				if (switchOn)
					Led.Brightness = Led.MaxBrightness;
				else
					Led.Brightness = 0;
			});
		}
	}
}
