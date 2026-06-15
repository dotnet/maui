using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
	internal override void RemapForControls(HashSet<Type> remapped)
	{
		if (remapped.Add(typeof(Stepper)))
		{
			base.RemapForControls(remapped);

			StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), MapInterval);
		}
	}

	internal static void MapInterval(IStepperHandler handler, IStepper stepper)
	{
		handler.UpdateValue(nameof(IStepper.Interval));
	}
}
