using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	static Stepper()
	{
		// Force VisualElement's static constructor to run first so base-level
		// mapper remappings are applied before these Control-specific ones.
#if DEBUG
		RemappingDebugHelper.AssertBaseClassForRemapping(typeof(Stepper), typeof(VisualElement));
#endif
		VisualElement.s_forceStaticConstructor = true;

		StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
