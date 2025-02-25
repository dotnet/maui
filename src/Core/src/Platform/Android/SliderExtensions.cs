﻿using System;
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
			if (slider.MinimumTrackColor is not null)
			{
				seekBar.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
				seekBar.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateMaximumTrackColor(this SeekBar seekBar, ISlider slider)
		{
			if (slider.MaximumTrackColor is not null)
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
			if (context is null || !seekBar.IsAlive())
				return;

			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource is not null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var result = await service.GetDrawableAsync(thumbImageSource, context);
				var thumbDrawable = result?.Value;

				if (thumbDrawable is not null)
				{
					SetThumbDrawable(seekBar, context, thumbDrawable);
				}
				else
				{
					SetDefaultThumb(seekBar, slider, context);
				}
			}
			else
			{
				SetDefaultThumb(seekBar, slider, context);
			}
		}

		static void SetThumbDrawable(SeekBar seekBar, Android.Content.Context context, Drawable thumbDrawable)
		{
			//Material 2 design spec - https://m2.material.io/components/sliders/android#discrete-slider
			const int TARGET_SIZE = 20; // 10 radius * 2
			int thumbSize = (int)context.ToPixels(TARGET_SIZE);

			Bitmap bitmap = Bitmap.CreateBitmap(thumbSize, thumbSize, Bitmap.Config.Argb8888!);
			Canvas canvas = new Canvas(bitmap);
			thumbDrawable.SetBounds(0, 0, thumbSize, thumbSize);
			thumbDrawable.Draw(canvas);

			BitmapDrawable finalDrawable = new BitmapDrawable(context.Resources, bitmap);
			seekBar.SetThumb(finalDrawable);
		}

		static void SetDefaultThumb(SeekBar seekBar, ISlider slider, Android.Content.Context context)
		{
			seekBar.SetThumb(context.GetDrawable(Resource.Drawable.abc_seekbar_thumb_material));

			if (slider.ThumbColor == null && context.Theme != null)
			{
				using var value = new TypedValue();
				if (context.Theme.ResolveAttribute(Android.Resource.Attribute.ColorAccent, value, true))
				{
					seekBar.Thumb?.SetColorFilter(new Color(value.Data), FilterMode.SrcIn);
				}
			}
			else
			{
				seekBar.UpdateThumbColor(slider);
			}
		}
	}
}