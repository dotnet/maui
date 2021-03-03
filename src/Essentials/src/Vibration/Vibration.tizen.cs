using System;
using System.Linq;
using Tizen.System;

namespace Microsoft.Maui.Essentials
{
	public static partial class Vibration
	{
		internal static bool IsSupported
			=> Vibrator.NumberOfVibrators > 0;

		static void PlatformVibrate(TimeSpan duration)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();
			Vibrator.Vibrators.FirstOrDefault()?.Vibrate((int)duration.TotalMilliseconds, 100);
		}

		static void PlatformCancel()
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();
			Vibrator.Vibrators.FirstOrDefault()?.Stop();
		}
	}
}
