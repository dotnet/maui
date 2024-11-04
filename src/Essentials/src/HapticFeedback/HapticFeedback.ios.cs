using System;
using UIKit;

namespace Microsoft.Maui.Devices
{
	partial class HapticFeedbackImplementation : IHapticFeedback
	{
		public bool IsSupported => true;

		public void Perform(HapticFeedbackType type)
		{
			switch (type)
			{
				case HapticFeedbackType.LongPress:
					LongPress();
					break;
				default:
					Click();
					break;
			}
		}

		void Click()
		{
			UIImpactFeedbackGenerator impact;
#if IOS17_5_OR_GREATER || MACCATALYST17_5_OR_GREATER
			if (OperatingSystem.IsIOSVersionAtLeast(17, 5) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 5))
			{
				impact = UIImpactFeedbackGenerator.GetFeedbackGenerator(UIImpactFeedbackStyle.Light, new UIView());
			}
			else
#endif
			{
				impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
			}
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}

		void LongPress()
		{
			UIImpactFeedbackGenerator impact;
#if IOS17_5_OR_GREATER || MACCATALYST17_5_OR_GREATER
			if (OperatingSystem.IsIOSVersionAtLeast(17, 5) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 5))
			{
				impact = UIImpactFeedbackGenerator.GetFeedbackGenerator(UIImpactFeedbackStyle.Medium, new UIView());
			}
			else
#endif
			{
				impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
			}
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}
	}
}
