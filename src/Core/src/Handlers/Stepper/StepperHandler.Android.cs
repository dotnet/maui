using System;
using Android.Views;
using Android.Widget;
using AButton = Android.Widget.Button;
using AOrientation = Android.Widget.Orientation;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Android.Views.View>, IAndroidStepperHandler
	{
		AButton? _downButton;
		AButton? _upButton;


		AButton? IAndroidStepperHandler.UpButton => _upButton;

		AButton? IAndroidStepperHandler.DownButton => _downButton;

		protected override Android.Views.View CreatePlatformView()
		{
			var stepperLayout = new MauiStepper(Context)
			{
				Orientation = (int)AOrientation.Horizontal,
				Focusable = true,
				DescendantFocusability = DescendantFocusability.AfterDescendants
			};

			StepperHandlerManager.CreateStepperButtons(this, out _downButton, out _upButton);

			if (_downButton != null)
				stepperLayout.AddView(_downButton, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));

			if (_upButton != null)
				stepperLayout.AddView(_upButton, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));

			return stepperLayout;
		}

		public static void MapMinimum(IStepperHandler handler, IStepper stepper)
		{
			(handler.PlatformView as MauiStepper)?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(IStepperHandler handler, IStepper stepper)
		{
			(handler.PlatformView as MauiStepper)?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(IStepperHandler handler, IStepper stepper)
		{
			(handler.PlatformView as MauiStepper)?.UpdateIncrement(stepper);
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			(handler.PlatformView as MauiStepper)?.UpdateValue(stepper);
		}

		AButton IAndroidStepperHandler.CreateButton()
		{
			if (Context == null)
				throw new ArgumentException("Context is null or empty", nameof(Context));

			var button = new AButton(Context);
			button.SetHeight((int)Context.ToPixels(10.0));
			return button;
		}
	}
}