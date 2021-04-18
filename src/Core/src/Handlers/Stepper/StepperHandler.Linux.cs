using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<ISlider, SpinButton>
	{
		protected override SpinButton CreateNativeView()
		{
			var adjustment = new Adjustment(0, 0, 1, 1, 1, 1);
			return new SpinButton(adjustment, 1, 1);
		}

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