using System;
using System.Linq;
using Tizen.System;

namespace Microsoft.Maui.Devices
{
	public partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> Vibrator.NumberOfVibrators > 0;

		public void Vibrate() 
			=> Vibrate(TimeSpan.FromMilliseconds(500));

		public void Vibrate(double duration) 
			=> Vibrate(TimeSpan.FromMilliseconds(duration));

		public void Vibrate(TimeSpan duration)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();
			Vibrator.Vibrators.FirstOrDefault()?.Vibrate((int)duration.TotalMilliseconds, 100);
		}

		public void Cancel()
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();
			Vibrator.Vibrators.FirstOrDefault()?.Stop();
		}
	}
}
