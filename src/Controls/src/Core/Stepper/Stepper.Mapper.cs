using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
	internal override void RemapForControls()
	{
		base.RemapForControls();
		s_remappedForControls.InvokeOnce();
	}

	static void RemapForControlsOnce()
	{
		StepperHandler.Mapper.AppendToMappingForControls(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
