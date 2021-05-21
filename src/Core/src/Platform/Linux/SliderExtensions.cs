using Gtk;

namespace Microsoft.Maui
{
	public static class SliderExtensions
	{
		public static void UpdateRange(this Scale nativeSlider, ISlider slider)
		{
			var minimum = slider.Minimum;
			var maximum = slider.Maximum;

			nativeSlider.SetRange(minimum, maximum);
		}

		public static void UpdateValue(this Scale nativeSlider, ISlider slider)
		{
			nativeSlider.Value = (float)slider.Value;
		}
	}
}
