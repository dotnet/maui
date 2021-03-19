using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		LinearLayout GetNativeStepper(StepperHandler stepperHandler) =>
			(LinearLayout)stepperHandler.View;

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

		async Task ValidateNativeBackgroundColor(IStepper stepper, Color color)
		{
			var expected = await GetValueAsync(stepper, handler => ((ColorDrawable)GetNativeStepper(handler).Background).Color.ToColor());
			Assert.Equal(expected, color);
		}
	}
}