using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public const double PlatformMaxValue = int.MaxValue;

		public static void UpdateMinimum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);
		internal static void UpdateMinimum(this MauiMaterialSlider mSlider, ISlider slider)
		{
			mSlider.ValueFrom = (float)slider.Minimum;
		}

		public static void UpdateMaximum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);
		internal static void UpdateMaximum(this MauiMaterialSlider mSlider, ISlider slider)
		{
			mSlider.ValueTo = (float)slider.Maximum;
		}

		public static void UpdateValue(this SeekBar seekBar, ISlider slider)
		{
			var min = slider.Minimum;
			var max = slider.Maximum;
			var value = slider.Value;

			seekBar.Progress = (int)((value - min) / (max - min) * PlatformMaxValue);
		}

		internal static void UpdateValue(this MauiMaterialSlider mSlider, ISlider slider)
		{
			mSlider.Value = (float)slider.Value;
		}

		public static void UpdateMinimumTrackColor(this SeekBar seekBar, ISlider slider)
		{
			if (slider.MinimumTrackColor != null)
			{
				seekBar.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
				seekBar.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		internal static void UpdateMinimumTrackColor(this MauiMaterialSlider mSlider, ISlider slider)
		{
			if (slider.MinimumTrackColor != null)
			{
				// mSlider.TickActiveTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
				// mSlider.ProgressTintMode = PorterDuff.Mode.SrcIn;
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

		internal static void UpdateMaximumTrackColor(this MauiMaterialSlider mSlider, ISlider slider)
		{
			if (slider.MaximumTrackColor != null)
			{
				// mSlider.ProgressBackgroundTintList = ColorStateList.ValueOf(slider.MaximumTrackColor.ToPlatform());
				// mSlider.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void UpdateThumbColor(this SeekBar seekBar, ISlider slider) =>
			seekBar.Thumb?.SetColorFilter(slider.ThumbColor, FilterMode.SrcIn);

		internal static void UpdateThumbColor(this MauiMaterialSlider mSlider, ISlider slider)
		{
			//mSlider.Thumb?.SetColorFilter(slider.ThumbColor, FilterMode.SrcIn);
		}

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
					seekBar.SetThumb(thumbDrawable);
			}
			else
			{
				seekBar.SetThumb(context.GetDrawable(Resource.Drawable.abc_seekbar_thumb_material));
				if (slider.ThumbColor is null && context.Theme is not null)
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

		internal static async Task UpdateThumbImageSourceAsync(this MauiMaterialSlider mSlider, ISlider slider, IImageSourceServiceProvider provider)
		{

		}
	}
}