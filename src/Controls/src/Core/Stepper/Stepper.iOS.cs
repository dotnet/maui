using System;

namespace Microsoft.Maui.Controls.Stepper;

public class Stepper.iOS
{
    internal static void MapInterval(IStepperHandler handler, IStepper view)
    {
       handler.PlatformView.UpdateIncrement(view);
    }
}
