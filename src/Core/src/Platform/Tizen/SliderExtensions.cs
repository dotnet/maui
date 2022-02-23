using Tizen.UIExtensions.NUI.GraphicsView;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public static void UpdateMinimum(this Slider platformSlider, ISlider slider)
		{
			platformSlider.Minimum = slider.Minimum;
		}

		public static void UpdateMaximum(this Slider platformSlider, ISlider slider)
		{
			platformSlider.Maximum = slider.Maximum;
		}

		public static void UpdateValue(this Slider platformSlider, ISlider slider)
		{
			platformSlider.Value = slider.Value;
		}

		public static void UpdateMinimumTrackColor(this Slider platformSlider, ISlider slider)
		{
			UpdateMinimumTrackColor(platformSlider, slider, null);
		}

		public static void UpdateMinimumTrackColor(this Slider platformSlider, ISlider slider, Color? defaultMinTrackColor)
		{
			if (slider.MinimumTrackColor == null)
			{
				if (defaultMinTrackColor != null)
					platformSlider.MinimumTrackColor = defaultMinTrackColor.ToPlatform();
			}
			else
				platformSlider.MinimumTrackColor = slider.MinimumTrackColor.ToPlatform();
		}

		public static void UpdateMaximumTrackColor(this Slider platformSlider, ISlider slider)
		{
			UpdateMaximumTrackColor(platformSlider, slider, null);
		}

		public static void UpdateMaximumTrackColor(this Slider platformSlider, ISlider slider, Color? defaultMaxTrackColor)
		{
			if (slider.MaximumTrackColor == null)
			{
				if (defaultMaxTrackColor != null)
					platformSlider.MaximumTrackColor = defaultMaxTrackColor.ToPlatform();
			}
			else
			{
				platformSlider.MaximumTrackColor = slider.MaximumTrackColor.ToPlatform();
			}
		}

		public static void UpdateThumbColor(this Slider platformSlider, ISlider slider)
		{
			UpdateThumbColor(platformSlider, slider, null);
		}

		public static void UpdateThumbColor(this Slider platformSlider, ISlider slider, Color? defaultThumbColor)
		{
			if (slider.ThumbColor == null)
			{
				if (defaultThumbColor != null)
					platformSlider.ThumbColor = defaultThumbColor.ToPlatform();
			}
			else
			{
				platformSlider.ThumbColor = slider.ThumbColor.ToPlatform();
			}
		}
	}
}