using CoreAnimation;
using UIKit;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this UIButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.SetNeedsDisplay();
		}

		public static void UpdateIsChecked(this UIButton nativeRadioButton, IRadioButton radioButton, CALayer? radioButtonLayer)
		{
			radioButtonLayer?.SetNeedsDisplay();
		}
	}
}
