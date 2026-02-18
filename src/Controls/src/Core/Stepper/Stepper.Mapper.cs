using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	static Stepper()
	{
		// Force VisualElement's static constructor to run first so base-level
		// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(Stepper), typeof(VisualElement));

		StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
