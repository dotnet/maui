using Gtk;

namespace Microsoft.Maui
{
	public static class SliderExtensions
	{
		public static void UpdateRange(this Range nativeSlider, IRange slider)
		{
			var minimum = slider.Minimum;
			var maximum = slider.Maximum;

			nativeSlider.SetRange(minimum, maximum);
		}

		public static void UpdateValue(this Range nativeSlider, IRange slider)
		{
			nativeSlider.Value = slider.Value;
		}
	}
}
