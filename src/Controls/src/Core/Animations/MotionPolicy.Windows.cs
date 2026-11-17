#nullable enable

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class MotionPolicy
	{
		private static partial bool PlatformIsReducedMotion() => !new global::Windows.UI.ViewManagement.UISettings().AnimationsEnabled;

		private static partial double PlatformDurationScale() => 1.0;
	}
}
