using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class HapticFeedback
	{
		internal static bool IsSupported => true;

		static void PlatformPerform(HapticFeedbackType type)
		{
			switch (type)
			{
				case HapticFeedbackType.LongPress:
					PlatformLongPress();
					break;
				default:
					PlatformClick();
					break;
			}
		}

		static void PlatformClick()
		{
			if (Platform.HasOSVersion(10, 0))
			{
				var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
				impact.Prepare();
				impact.ImpactOccurred();
				impact.Dispose();
			}
		}

		static void PlatformLongPress()
		{
			if (Platform.HasOSVersion(10, 0))
			{
				var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
				impact.Prepare();
				impact.ImpactOccurred();
				impact.Dispose();
			}
		}
	}
}
