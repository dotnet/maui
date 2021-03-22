using System;
using Android.Views;
using Android.Widget;
using AButton = Android.Widget.Button;
using AOrientation = Android.Widget.Orientation;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : AbstractViewHandler<IStepper, LinearLayout>, IStepperHandler
	{
		AButton? _downButton;
		AButton? _upButton;

		IStepper? IStepperHandler.VirtualView => VirtualView;

		AButton? IStepperHandler.UpButton => _upButton;

		AButton? IStepperHandler.DownButton => _downButton;

		protected override LinearLayout CreateNativeView()
		{
			var stepperLayout = new LinearLayout(Context)
			{
				Orientation = AOrientation.Horizontal,
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

		public static void MapMinimum(StepperHandler handler, IStepper stepper)
		{
			handler.TypedNativeView?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(StepperHandler handler, IStepper stepper)
		{
			handler.TypedNativeView?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(StepperHandler handler, IStepper stepper)
		{
			handler.TypedNativeView?.UpdateIncrement(stepper);
		}

		public static void MapValue(StepperHandler handler, IStepper stepper)
		{
			handler.TypedNativeView?.UpdateValue(stepper);
		}

		AButton IStepperHandler.CreateButton()
		{
			if (Context == null)
				throw new ArgumentException("Context is null or empty", nameof(Context));

			var button = new AButton(Context);
			button.SetHeight((int)Context.ToPixels(10.0));
			return button;
		}
	}
}