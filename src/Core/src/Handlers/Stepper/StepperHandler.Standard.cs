using System;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : WidgetHandler<IStepper, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapMinimum(IFrameworkElementHandler handler, IStepper stepper) { }
		public static void MapMaximum(IFrameworkElementHandler handler, IStepper stepper) { }
		public static void MapIncrement(IFrameworkElementHandler handler, IStepper stepper) { }
		public static void MapValue(IFrameworkElementHandler handler, IStepper stepper) { }
	}
}