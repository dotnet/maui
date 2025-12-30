using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using MSlider = Google.Android.Material.Slider.Slider;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public const double PlatformMaxValue = int.MaxValue;

		public static void UpdateMinimum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		internal static void UpdateMinimum(this MSlider mSlider, ISlider slider)
		{
			mSlider.ValueFrom = (float)slider.Minimum;
		}

		public static void UpdateMaximum(this SeekBar seekBar, ISlider slider) => UpdateValue(seekBar, slider);

		internal static void UpdateMaximum(this MSlider mSlider, ISlider slider)
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

		internal static void UpdateValue(this MSlider mSlider, ISlider slider)
		{
			if ((float)slider.Value != mSlider.Value)
			{
				mSlider.Value = (float)slider.Value;
			}
		}

		public static void UpdateMinimumTrackColor(this SeekBar seekBar, ISlider slider)
		{
			if (slider.MinimumTrackColor != null)
			{
				seekBar.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
				seekBar.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		internal static void UpdateMinimumTrackColor(this MSlider mSlider, ISlider slider)
		{
			if (slider.MinimumTrackColor is not null)
			{
				mSlider.TrackActiveTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToPlatform());
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

		internal static void UpdateMaximumTrackColor(this MSlider mSlider, ISlider slider)
		{
			if (slider.MaximumTrackColor is not null)
			{
				mSlider.TrackInactiveTintList = ColorStateList.ValueOf(slider.MaximumTrackColor.ToPlatform());
			}
		}
		public static void UpdateThumbColor(this SeekBar seekBar, ISlider slider) =>
			seekBar.Thumb?.SetColorFilter(slider.ThumbColor, FilterMode.SrcIn);

		internal static void UpdateThumbColor(this MSlider mSlider, ISlider slider)
		{
			if (slider.ThumbImageSource is not null && slider.Handler is not null)
			{
				var provider = slider.Handler.GetRequiredService<IImageSourceServiceProvider>();
				mSlider.UpdateThumbImageSourceAsync(slider, provider)
					.FireAndForget();
			}
			else if (slider.ThumbColor is not null)
			{
				mSlider.ThumbTintList = ColorStateList.ValueOf(slider.ThumbColor.ToPlatform());
			}
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

		internal static async Task UpdateThumbImageSourceAsync(this MSlider mSlider, ISlider slider,
   IImageSourceServiceProvider provider)
		{
			var context = mSlider.Context;

			if (context is null)
			{
				return;
			}

			var thumbImageSource = slider.ThumbImageSource;

			if (thumbImageSource is not null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var result = await service.GetDrawableAsync(thumbImageSource, context);

				var thumbDrawable = result?.Value;

				if (mSlider.IsAlive() && thumbDrawable is not null)
				{
					if (slider.ThumbColor is not null)
					{
						// Mutate the drawable to avoid affecting other instances
						thumbDrawable = thumbDrawable.Mutate();
						thumbDrawable.SetColorFilter(slider.ThumbColor.ToPlatform(), FilterMode.SrcIn);
					}
					mSlider.SetCustomThumbDrawable(thumbDrawable);
				}
			}
		}
	}
}