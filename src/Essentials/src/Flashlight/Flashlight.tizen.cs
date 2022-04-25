using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.System;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		internal static bool IsSupported
			=> PlatformUtils.GetFeatureInfo<bool>("camera.back.flash");

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
