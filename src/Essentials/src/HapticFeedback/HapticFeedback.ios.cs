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
#if IOS17_4_OR_GREATER || MACCATALYST17_4_OR_GREATER
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
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
#if IOS17_4_OR_GREATER || MACCATALYST17_4_OR_GREATER
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
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
