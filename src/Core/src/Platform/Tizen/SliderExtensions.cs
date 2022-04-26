using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;

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

		public static void UpdateMinimumTrackColor(this Slider platformSlider, ISlider slider, EColor? defaultMinTrackColor)
		{
			if (slider.MinimumTrackColor == null)
			{
				if (defaultMinTrackColor != null)
					platformSlider.SetBarColor(defaultMinTrackColor.Value);
			}
			else
				platformSlider.SetBarColor(slider.MinimumTrackColor.ToPlatformEFL());
		}

		public static void UpdateMaximumTrackColor(this Slider platformSlider, ISlider slider)
		{
			UpdateMaximumTrackColor(platformSlider, slider, null);
		}

		public static void UpdateMaximumTrackColor(this Slider platformSlider, ISlider slider, EColor? defaultMaxTrackColor)
		{
			if (slider.MaximumTrackColor == null)
			{
				if (defaultMaxTrackColor != null)
					platformSlider.SetBackgroundColor(defaultMaxTrackColor.Value);
			}
			else
			{
				platformSlider.SetBackgroundColor(slider.MaximumTrackColor.ToPlatformEFL());
			}
		}

		public static void UpdateThumbColor(this Slider platformSlider, ISlider slider)
		{
			UpdateThumbColor(platformSlider, slider, null);
		}

		public static void UpdateThumbColor(this Slider platformSlider, ISlider slider, EColor? defaultThumbColor)
		{
			if (slider.ThumbColor == null)
			{
				if (defaultThumbColor != null)
					platformSlider.SetHandlerColor(defaultThumbColor.Value);
			}
			else
			{
				platformSlider.SetHandlerColor(slider.ThumbColor.ToPlatformEFL());
			}
		}
	}
}