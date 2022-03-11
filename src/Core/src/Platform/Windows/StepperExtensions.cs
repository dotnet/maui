using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Minimum = stepper.Minimum;
		}

		public static void UpdateMaximum(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Maximum = stepper.Maximum;
		}

		public static void UpdateInterval(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Increment = stepper.Interval;
		}

		public static void UpdateValue(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Value = stepper.Value;
		}

		public static void UpdateBackground(this MauiStepper platformStepper, IStepper stepper)
		{
			var background = stepper?.Background;
			if (background == null)
			{
				return;
			}

			platformStepper.ButtonBackground = background.ToPlatform();
		}
	}
}