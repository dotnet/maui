using System;
using AudioToolbox;

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported => true;

		void PlatformVibrate() =>
			SystemSound.Vibrate.PlaySystemSound();

		void PlatformVibrate(TimeSpan duration) =>
			SystemSound.Vibrate.PlaySystemSound();

		void PlatformCancel()
		{
		}
	}
}
