using Android.Widget;
using AButton = Android.Widget.Button;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this MauiStepper linearLayout, IStepper stepper)
		{
			UpdateButtons(linearLayout, stepper);
		}

		public static void UpdateMaximum(this MauiStepper linearLayout, IStepper stepper)
		{
			UpdateButtons(linearLayout, stepper);
		}

		public static void UpdateIncrement(this MauiStepper linearLayout, IStepper stepper)
		{
			UpdateButtons(linearLayout, stepper);
		}

		public static void UpdateValue(this MauiStepper linearLayout, IStepper stepper)
		{
			UpdateButtons(linearLayout, stepper);
		}

		public static void UpdateIsEnabled(this MauiStepper linearLayout, IStepper stepper)
		{
			UpdateButtons(linearLayout, stepper);
		}

		internal static void UpdateButtons(this MauiStepper linearLayout, IStepper stepper)
		{
			AButton? downButton = null;
			AButton? upButton = null;

			for (int i = 0; i < linearLayout?.ChildCount; i++)
			{
				var childButton = linearLayout.GetChildAt(i) as AButton;

				if (childButton?.Text == "－")
					downButton = childButton;

				if (childButton?.Text == "＋")
					upButton = childButton;
			}

			StepperHandlerManager.UpdateButtons(stepper, downButton, upButton);
		}
	}
}