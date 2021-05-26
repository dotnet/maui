using System;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		LinearLayout GetNativeStepper(StepperHandler stepperHandler) =>
			stepperHandler.NativeView;

		double GetNativeValue(StepperHandler stepperHandler)
		{
			var nativeView = GetNativeStepper(stepperHandler);
			var nativeButton = nativeView.GetChildAt(0);

			if (nativeButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Value;

			return 0;
		}

		double GetNativeMaximum(StepperHandler stepperHandler)
		{
			var nativeView = GetNativeStepper(stepperHandler);
			var nativeButton = nativeView.GetChildAt(0);

			if (nativeButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Maximum;

			return 0;
		}

		double GetNativeMinimum(StepperHandler stepperHandler)
		{
			var nativeView = GetNativeStepper(stepperHandler);
			var nativeButton = nativeView.GetChildAt(0);

			if (nativeButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Minimum;

			return 0;
		}

		Task ValidateHasColor(IStepper stepper, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeStepper = GetNativeStepper(CreateHandler(stepper));
				action?.Invoke();
				nativeStepper.AssertContainsColor(color);
			});
		}
	}
}