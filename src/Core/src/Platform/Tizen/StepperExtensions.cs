using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this Stepper nativeStepper, IStepper stepper)
		{
			platformStepper.Minimum = stepper.Minimum;
		}

		public static void UpdateMaximum(this Stepper nativeStepper, IStepper stepper)
		{
			platformStepper.Maximum = stepper.Maximum;
		}

		public static void UpdateIncrement(this Stepper nativeStepper, IStepper stepper)
		{
			nativeStepper.Increment = stepper.Interval;
		}

		public static void UpdateValue(this Stepper nativeStepper, IStepper stepper)
		{
			if (platformStepper.Value != stepper.Value)
				platformStepper.Value = stepper.Value;
		}
	}
}