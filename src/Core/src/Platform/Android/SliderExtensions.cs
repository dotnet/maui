using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Graphics.Platform;

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

			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var result = await service.GetDrawableAsync(thumbImageSource, context);
				var thumbDrawable = result?.Value;

				if (seekBar.IsAlive() && thumbDrawable != null)
				{
					if (thumbDrawable is BitmapDrawable bitmapDrawable && bitmapDrawable.Bitmap is { } bitmap)
					{
						// Define the target size for the thumb image
						const int TARGET_SIZE = 48; // 48dp - default size of the thumb in Android

						// Resize the bitmap to the target size
						var thumbImage = bitmap.Downsize(TARGET_SIZE);

						// Set the resized thumb image
						seekBar.SetThumb(new BitmapDrawable(context.Resources, thumbImage));
					}
					else
					{
						// Set the original drawable if it's not a BitmapDrawable or the bitmap is null
						seekBar.SetThumb(thumbDrawable);
					}
				}
			}
			else
			{
				seekBar.SetThumb(context.GetDrawable(Resource.Drawable.abc_seekbar_thumb_material));
				if (slider.ThumbColor is null && context.Theme is not null)
				{
					using var value = new TypedValue();
					context.Theme.ResolveAttribute(Android.Resource.Attribute.ColorAccent, value, true);
					var color = new Color(value.Data);
					seekBar.Thumb?.SetColorFilter(color, FilterMode.SrcIn);
				}
				else
				{
					seekBar.UpdateThumbColor(slider);
				}
			}
		}
	}
}