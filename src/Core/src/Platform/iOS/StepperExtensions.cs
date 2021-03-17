using UIKit;

namespace Microsoft.Maui
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this UIStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.MinimumValue = stepper.Minimum;
		}

		public static void UpdateMaximum(this UIStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.MaximumValue = stepper.Maximum;
		}

		public static void UpdateIncrement(this UIStepper nativeStepper, IStepper stepper)
		{
			var increment = stepper.Increment;

			if (increment > 0)
				nativeStepper.StepValue = stepper.Increment;
		}

		public static void UpdateValue(this UIStepper nativeStepper, IStepper stepper)
		{
			if (nativeStepper.Value != stepper.Value)
				nativeStepper.Value = stepper.Value;
		}
	}
}