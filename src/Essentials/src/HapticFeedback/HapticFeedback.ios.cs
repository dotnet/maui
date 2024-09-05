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
#pragma warning disable CA1422 // obsolete in MacCatalyst 13, iOS 13
			var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
#pragma warning restore CA1422
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}

		void LongPress()
		{
#pragma warning disable CA1422 // obsolete in MacCatalyst 13, iOS 13
			var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
#pragma warning restore CA1422
			impact.Prepare();
			impact.ImpactOccurred();
			impact.Dispose();
		}
	}
}
