using System;

namespace Microsoft.Maui.Controls;

public class Stepper.Android
{
    internal static void MapInterval(IStepperHandler handler, IStepper view)
    {
       handler.PlatformView.UpdateIncrement(view);
    }
}
