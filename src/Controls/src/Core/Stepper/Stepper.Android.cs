using System;

namespace Microsoft.Maui.Controls;

public partial class Stepper
{
    internal static void MapInterval(IStepperHandler handler, IStepper view)
    {
       handler.PlatformView.UpdateIncrement(view);
    }
}
