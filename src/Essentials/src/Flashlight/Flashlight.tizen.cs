using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.System;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		internal static bool IsSupported
			=> PlatformUtils.GetFeatureInfo<bool>("camera.back.flash");

		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public Task<bool> IsSupportedAsync() => Task.FromResult(IsSupported);

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
