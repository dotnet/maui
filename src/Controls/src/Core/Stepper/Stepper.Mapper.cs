using System;
using System.Threading;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	static int s_remappedForControls;

	internal override void RemapForControls()
	{
		if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
			return;

		base.RemapForControls();

			StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
