using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this UIStepper platformStepper, IStepper stepper)
		{
			platformStepper.MinimumValue = stepper.Minimum;
		}

		public static void UpdateMaximum(this UIStepper platformStepper, IStepper stepper)
		{
			platformStepper.MaximumValue = stepper.Maximum;
		}

		public static void UpdateIncrement(this UIStepper platformStepper, IStepper stepper)
		{
			var increment = stepper.Interval;

			if (increment > 0)
				platformStepper.StepValue = stepper.Interval;
		}

		public static void UpdateValue(this UIStepper platformStepper, IStepper stepper)
		{
			if (platformStepper.Value != stepper.Value)
				platformStepper.Value = stepper.Value;
		}
	}
}