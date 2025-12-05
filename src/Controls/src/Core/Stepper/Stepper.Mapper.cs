using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	static Stepper() => RemapForControls();

	private static new void RemapForControls()
	{
		VisualElement.RemapIfNeeded();

		StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
