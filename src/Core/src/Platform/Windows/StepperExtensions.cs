using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this MauiStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.Minimum = stepper.Minimum;
		}

		public static void UpdateMaximum(this MauiStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.Maximum = stepper.Maximum;
		}

		public static void UpdateInterval(this MauiStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.Increment = stepper.Interval;
		}

		public static void UpdateValue(this MauiStepper nativeStepper, IStepper stepper)
		{
			nativeStepper.Value = stepper.Value;
		}

		public static void UpdateBackground(this MauiStepper nativeStepper, IStepper stepper)
		{
			var background = stepper?.Background;
			if (background == null)
			{
				return;
			}

			nativeStepper.ButtonBackground = background.ToNative();
		}
	}
}