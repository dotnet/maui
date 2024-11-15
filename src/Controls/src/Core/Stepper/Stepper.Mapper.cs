using System;

namespace Microsoft.Maui.Controls;
public partial class Stepper
{
    internal static new void RemapForControls()
    {
        StepperHandler.Mapper.AppendToMapping(nameof(Stepper.Increment), Stepper.MapInterval);
    }
}
