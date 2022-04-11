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
			var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}

		void LongPress()
		{
			var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}
	}
}
