using System;
using AudioToolbox;

namespace Microsoft.Maui.Essentials
{
	public static partial class Vibration
	{
		internal static bool IsSupported => true;

		static void PlatformVibrate(TimeSpan duration) =>
			SystemSound.Vibrate.PlaySystemSound();

		static void PlatformCancel()
		{
		}
	}
}
