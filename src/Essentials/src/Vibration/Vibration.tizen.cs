using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Tizen.System;

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> Vibrator.NumberOfVibrators > 0;

		void PlatformVibrate()
			=> Vibrate(TimeSpan.FromMilliseconds(500));

		void PlatformVibrate(TimeSpan duration)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			Vibrator.Vibrators.FirstOrDefault()?.Vibrate((int)duration.TotalMilliseconds, 100);
		}

		void PlatformCancel()
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			Vibrator.Vibrators.FirstOrDefault()?.Stop();
		}
	}
}
