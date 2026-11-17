#nullable enable
using System;
using Android.Animation;

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class MotionPolicy
	{
		// AreAnimatorsEnabled reflects the effective scale, including battery-saver policies that disable
		// animation while Settings.Global still reports 1 (Android spike, condition C4b).
		private static partial bool PlatformIsReducedMotion()
			=> OperatingSystem.IsAndroidVersionAtLeast(26) && !ValueAnimator.AreAnimatorsEnabled();

		private static partial double PlatformDurationScale()
			=> OperatingSystem.IsAndroidVersionAtLeast(33) ? ValueAnimator.DurationScale : 1.0;
	}
}
