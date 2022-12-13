using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		MauiStepper GetNativeStepper(StepperHandler stepperHandler) =>
		stepperHandler.PlatformView;

		double GetPlatformValue(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Value;

		double GetNativeMaximum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Maximum;

		double GetNativeMinimum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Minimum;
	}
}