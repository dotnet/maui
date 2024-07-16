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
#if NET8_0
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
			{
				impact = UIImpactFeedbackGenerator.GetFeedbackGenerator(UIImpactFeedbackStyle.Light, new UIView());
			}
			else
			{
				impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
			}
#else
				impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
#endif
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}

		void LongPress()
		{
			UIImpactFeedbackGenerator impact;

#if NET8_0
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
			{
				impact = UIImpactFeedbackGenerator.GetFeedbackGenerator(UIImpactFeedbackStyle.Medium, new UIView());
			}
			else
			{
				impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
			}
#else
		impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
#endif
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}
	}
}
