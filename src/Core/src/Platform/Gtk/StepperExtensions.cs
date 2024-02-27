using Gtk;

namespace Microsoft.Maui
{
	public static class StepperExtensions
	{
		public static void UpdateRange(this SpinButton platformView, IRange slider)
		{
			var minimum = slider.Minimum;
			var maximum = slider.Maximum;

			platformView.SetRange(minimum, maximum);
		}

		public static void UpdateValue(this SpinButton platformView, IRange slider)
		{
			platformView.Value = slider.Value;
		}
		public static void UpdateIncrement(this SpinButton platformView, IStepper slider)
		{
			platformView.SetIncrements(slider.Interval,1);
		}
	}
}