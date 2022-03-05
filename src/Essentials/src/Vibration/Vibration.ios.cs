using System;
using AudioToolbox;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class VibrationImplementation : IVibration
	{
		public bool IsSupported => true;

		public void Vibrate() 
			=> Vibrate(TimeSpan.FromMilliseconds(500));

		public void Vibrate(double duration) 
			=> Vibrate(TimeSpan.FromMilliseconds(duration));

		public void Vibrate(TimeSpan duration) =>
			SystemSound.Vibrate.PlaySystemSound();

		public void Cancel()
		{
		}
	}
}
