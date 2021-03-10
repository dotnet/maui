using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using Microsoft.Maui;

namespace Microsoft.Maui
{
	public static class SliderExtensions
	{
		public const double NativeMaxValue = int.MaxValue;

		public static void UpdateMinimum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		public static void UpdateMaximum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		public static void UpdateValue(this SeekBar seekBar, ISlider slider)
		{
			var min = slider.Minimum;
			var max = slider.Maximum;
			var value = slider.Value;

			seekBar.Progress = (int)((value - min) / (max - min) * NativeMaxValue);
		}

		public static void UpdateMinimumTrackColor(this SeekBar seekBar, ISlider slider) =>
			UpdateMinimumTrackColor(seekBar, slider, null, null);

		public static void UpdateMinimumTrackColor(this SeekBar seekBar, ISlider slider, ColorStateList? defaultProgressTintList, PorterDuff.Mode? defaultProgressTintMode)
		{
			if (slider.MinimumTrackColor == Maui.Color.Default)
			{
				if (defaultProgressTintList != null)
					seekBar.ProgressTintList = defaultProgressTintList;

				if (defaultProgressTintMode != null)
					seekBar.ProgressTintMode = defaultProgressTintMode;
			}
			else
			{
				seekBar.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToNative());
				seekBar.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateMaximumTrackColor(this SeekBar seekBar, ISlider slider) =>
			UpdateMaximumTrackColor(seekBar, slider, null, null);

		public static void UpdateMaximumTrackColor(this SeekBar seekBar, ISlider slider, ColorStateList? defaultProgressBackgroundTintList, PorterDuff.Mode? defaultProgressBackgroundTintMode)
		{
			if (slider.MaximumTrackColor == Maui.Color.Default)
			{
				if (defaultProgressBackgroundTintList != null)
					seekBar.ProgressBackgroundTintList = defaultProgressBackgroundTintList;

				if (defaultProgressBackgroundTintMode != null)
					seekBar.ProgressBackgroundTintMode = defaultProgressBackgroundTintMode;
			}
			else
			{
				seekBar.ProgressBackgroundTintList = ColorStateList.ValueOf(slider.MaximumTrackColor.ToNative());
				seekBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateThumbColor(this SeekBar seekBar, ISlider slider) =>
			UpdateThumbColor(seekBar, slider);

		public static void UpdateThumbColor(this SeekBar seekBar, ISlider slider, ColorFilter? defaultThumbColorFilter) =>
			seekBar.Thumb?.SetColorFilter(slider.ThumbColor, FilterMode.SrcIn, defaultThumbColorFilter);
	}
}