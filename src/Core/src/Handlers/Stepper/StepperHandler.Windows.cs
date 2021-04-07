using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Button>
	{
		protected override Button CreateNativeView() => new Button();

		[MissingMapper]
		public static void MapMinimum(IViewHandler handler, IStepper stepper) { }

		[MissingMapper]
		public static void MapMaximum(IViewHandler handler, IStepper stepper) { }

		[MissingMapper]
		public static void MapIncrement(IViewHandler handler, IStepper stepper) { }

		[MissingMapper]
		public static void MapValue(IViewHandler handler, IStepper stepper) { }
	}
}