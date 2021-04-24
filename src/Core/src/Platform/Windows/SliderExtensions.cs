#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public static class SliderExtensions
	{
		public static void UpdateMinimum(this Slider nativeSlider, ISlider slider)
		{
			nativeSlider.Minimum = slider.Minimum;
		}

		public static void UpdateMaximum(this Slider nativeSlider, ISlider slider)
		{
			nativeSlider.Maximum = slider.Maximum;
		}

		public static void UpdateValue(this Slider nativeSlider, ISlider slider)
		{
			if (nativeSlider.Value != slider.Value)
				nativeSlider.Value = slider.Value;
		}

		public static void UpdateMinimumTrackColor(this Slider nativeSlider, ISlider slider, Brush? defaultForegroundColor)
		{
			if (slider.MinimumTrackColor.IsDefault())
				nativeSlider.Foreground = defaultForegroundColor;
			else
				nativeSlider.Foreground = slider.MinimumTrackColor.ToNative();
		}

		public static void UpdateMaximumTrackColor(this Slider nativeSlider, ISlider slider, Brush? defaultBackgroundColor)
		{
			if (slider.MaximumTrackColor.IsDefault())
				nativeSlider.Background = defaultBackgroundColor;
			else
				nativeSlider.Background = slider.MaximumTrackColor.ToNative();
		}
	}
}