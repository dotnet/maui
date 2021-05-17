using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;

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
			nativeSlider.Value = slider.Value;
		}

		public static void UpdateMinimumTrackColor(this Slider nativeSlider, ISlider slider)
		{
			UpdateMinimumTrackColor(nativeSlider, slider, null);
		}

		public static void UpdateMinimumTrackColor(this Slider nativeSlider, ISlider slider, EColor? defaultMinTrackColor)
		{
			if (slider.MinimumTrackColor == null)
			{
				if (defaultMinTrackColor != null)
					nativeSlider.SetBarColor(defaultMinTrackColor.Value);
			}
			else
				nativeSlider.SetBarColor(slider.MinimumTrackColor.ToNativeEFL());
		}

		public static void UpdateMaximumTrackColor(this Slider nativeSlider, ISlider slider)
		{
			UpdateMaximumTrackColor(nativeSlider, slider, null);
		}

		public static void UpdateMaximumTrackColor(this Slider nativeSlider, ISlider slider, EColor? defaultMaxTrackColor)
		{
			if (slider.MaximumTrackColor == null)
			{
				if (defaultMaxTrackColor != null)
					nativeSlider.SetBackgroundColor(defaultMaxTrackColor.Value);
			}
			else
			{
				nativeSlider.SetBackgroundColor(slider.MaximumTrackColor.ToNativeEFL());
			}
		}

		public static void UpdateThumbColor(this Slider nativeSlider, ISlider slider)
		{
			UpdateThumbColor(nativeSlider, slider, null);
		}

		public static void UpdateThumbColor(this Slider nativeSlider, ISlider slider, EColor? defaultThumbColor)
		{
			if (slider.ThumbColor == null)
			{
				if (defaultThumbColor != null)
					nativeSlider.SetHandlerColor(defaultThumbColor.Value);
			}
			else
			{
				nativeSlider.SetHandlerColor(slider.ThumbColor.ToNativeEFL());
			}
		}
	}
}