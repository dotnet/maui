using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		UIStepper GetNativeStepper(StepperHandler stepperHandler) =>
			stepperHandler.PlatformView;

		double GetPlatformValue(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Value;

		double GetNativeMaximum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MaximumValue;

		double GetNativeMinimum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MinimumValue;

		Task ValidateHasColor(IStepper stepper, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformStepper = GetNativeStepper(CreateHandler(stepper));
				action?.Invoke();
				platformStepper.AssertContainsColorAsync(color);
			});
		}
	}
}