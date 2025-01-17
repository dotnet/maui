using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public const double PlatformMaxValue = int.MaxValue;

		public static void UpdateMinimum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		public static void UpdateMaximum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		public static void UpdateValue(this SeekBar seekBar, ISlider slider)
		{
			var min = slider.Minimum;
			var max = slider.Maximum;
			var value = slider.Value;

			seekBar.Progress = (int)((value - min) / (max - min) * PlatformMaxValue);
		}

		public static void UpdateMinimumTrackColor(this SeekBar seekBar, ISlider slider)
		{
			if (slider.MinimumTrackColor != null)
			{
				seekBar.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
				seekBar.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateMaximumTrackColor(this SeekBar seekBar, ISlider slider)
		{
			if (slider.MaximumTrackColor != null)
			{
				seekBar.ProgressBackgroundTintList = ColorStateList.ValueOf(slider.MaximumTrackColor.ToPlatform());
				seekBar.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateThumbColor(this SeekBar seekBar, ISlider slider) =>
			seekBar.Thumb?.SetColorFilter(slider.ThumbColor, FilterMode.SrcIn);

		public static async Task UpdateThumbImageSourceAsync(this SeekBar seekBar, ISlider slider, IImageSourceServiceProvider provider)
		{
			var context = seekBar.Context;
			if (context == null)
				return;

			var defaultThumbDrawable = context.GetDrawable(Resource.Drawable.abc_seekbar_thumb_material);

			int defaultThumbWidth = defaultThumbDrawable!.IntrinsicWidth;
			int defaultThumbHeight = defaultThumbDrawable.IntrinsicHeight;

			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var result = await service.GetDrawableAsync(thumbImageSource, context);
				var thumbDrawable = result?.Value;

				if (seekBar.IsAlive() && thumbDrawable != null)
				{
					using var value = new TypedValue();
					context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorAccent, value, true);
					var color = new Color(value.Data);
					seekBar.Thumb?.SetColorFilter(color, FilterMode.SrcIn);
				}
				else
				{
					seekBar.UpdateThumbColor(slider);
				}
			}
		}

		static Bitmap ResizeBitmap(Bitmap originalBitmap, int targetWidth, int targetHeight)
		{
			return Bitmap.CreateScaledBitmap(originalBitmap, targetWidth, targetHeight, true);
		}
	}
}