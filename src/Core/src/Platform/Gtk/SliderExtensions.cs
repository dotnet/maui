using Gtk;

namespace Microsoft.Maui
{
	public static class SliderExtensions
	{
		public static void UpdateRange(this Range platformView, IRange slider)
		{
			var minimum = slider.Minimum;
			var maximum = slider.Maximum;

			platformView.SetRange(minimum, maximum);
		}

		public static void UpdateValue(this Range platformView, IRange slider)
		{
			platformView.Value = slider.Value;
		}
	}
}
