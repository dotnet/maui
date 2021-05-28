using Gtk;

namespace Microsoft.Maui
{
	public static class StepperExtensions
	{
		public static void UpdateRange(this SpinButton nativeSlider, IRange slider)
		{
			var minimum = slider.Minimum;
			var maximum = slider.Maximum;

			nativeSlider.SetRange(minimum, maximum);
		}

		public static void UpdateValue(this SpinButton nativeSlider, IRange slider)
		{
			nativeSlider.Value = slider.Value;
		}
		public static void UpdateIncrement(this SpinButton nativeSlider, IRange slider)
		{
			nativeSlider.SetIncrements(slider.Value,1);
		}
	}
}