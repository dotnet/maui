using System;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		[Fact(DisplayName = "IsEnabled Initializes Correctly")]
		public async Task IsEnabledInitializesCorrectly()
		{
			var stepper = new StepperStub()
			{
				Minimum = 0,
				Maximum = 50,
				IsEnabled = false
			};

			await ValidatePropertyInitValue(stepper, () => stepper.IsEnabled, GetNativeIsEnabled, stepper.IsEnabled);
		}

		LinearLayout GetNativeStepper(StepperHandler stepperHandler) =>
			stepperHandler.PlatformView;

		double GetPlatformValue(StepperHandler stepperHandler)
		{
			var platformView = GetNativeStepper(stepperHandler);
			var platformButton = platformView.GetChildAt(0);

			if (platformButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Value;

			return 0;
		}

		double GetNativeMaximum(StepperHandler stepperHandler)
		{
			var platformView = GetNativeStepper(stepperHandler);
			var platformButton = platformView.GetChildAt(0);

			if (platformButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Maximum;

			return 0;
		}

		double GetNativeMinimum(StepperHandler stepperHandler)
		{
			var platformView = GetNativeStepper(stepperHandler);
			var platformButton = platformView.GetChildAt(0);

			if (platformButton?.Tag is StepperHandlerHolder handlerHolder)
				return handlerHolder.StepperHandler.VirtualView.Minimum;

			return 0;
		}

		bool GetNativeIsEnabled(StepperHandler stepperHandler)
		{
			var platformView = GetNativeStepper(stepperHandler);

			var minimumButton = platformView.GetChildAt(0);
			var maximumButton = platformView.GetChildAt(1);

			return minimumButton.Enabled && maximumButton.Enabled;
		}
	}
}