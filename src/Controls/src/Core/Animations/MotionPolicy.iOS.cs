#nullable enable
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class MotionPolicy
	{
		private static partial bool PlatformIsReducedMotion() => UIAccessibility.IsReduceMotionEnabled;

		private static partial double PlatformDurationScale() => 1.0;
	}
}
