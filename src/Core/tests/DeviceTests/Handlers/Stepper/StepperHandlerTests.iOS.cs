using System.Threading.Tasks;
using UIKit;
using Xunit;
using Microsoft.Maui.Handlers;

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

		async Task ValidateNativeBackgroundColor(IStepper stepper, Color color)
		{
			var expected = await GetValueAsync(stepper, handler => GetNativeStepper(handler).BackgroundColor.ToColor());
			Assert.Equal(expected, color);
		}
	}
}