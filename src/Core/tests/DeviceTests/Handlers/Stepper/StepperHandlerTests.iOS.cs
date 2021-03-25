using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;
using UIKit;
using System;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		UIStepper GetNativeStepper(StepperHandler stepperHandler) =>
			(UIStepper)stepperHandler.View;

		double GetNativeValue(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Value;

		double GetNativeMaximum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MaximumValue;

		double GetNativeMinimum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MinimumValue;

		Task ValidateNativeBackground(IStepper stepper, SolidColorBrush brush, Action action = null) =>
			ValidateHasColor(stepper, brush.Color, action);

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