using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	internal static new void RemapForControls()
	{
		StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
