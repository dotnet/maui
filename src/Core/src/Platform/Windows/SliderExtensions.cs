#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		static void UpdateIncrement(this MauiSlider nativeSlider, ISlider slider)
		{
			double stepping = Math.Min((slider.Maximum - slider.Minimum) / 1000, 1);
			nativeSlider.StepFrequency = stepping;
			nativeSlider.SmallChange = stepping;
		}

		public static void UpdateMinimum(this MauiSlider nativeSlider, ISlider slider)
		{
			nativeSlider.Minimum = slider.Minimum;
			nativeSlider.UpdateIncrement(slider);
		}

		public static void UpdateMaximum(this MauiSlider nativeSlider, ISlider slider)
		{
			nativeSlider.Maximum = slider.Maximum;
			nativeSlider.UpdateIncrement(slider);
		}

		public static void UpdateValue(this MauiSlider nativeSlider, ISlider slider)
		{
			if (nativeSlider.Value != slider.Value)
				nativeSlider.Value = slider.Value;
		}

		public static void UpdateMinimumTrackColor(this MauiSlider platformSlider, ISlider slider, Brush? defaultMinimumTrackColor)
		{
			var brush = slider.MinimumTrackColor.IsDefault() ? defaultMinimumTrackColor : slider.MinimumTrackColor.ToPlatform();


			platformSlider.Resources["SliderTrackValueFill"] = brush;
			platformSlider.Resources["SliderTrackValueFilllPointerOver"] = brush;
			platformSlider.Resources["SliderTrackValueFillPressed"] = brush;
			platformSlider.Resources["SliderTrackValueFillDisabled"] = brush;

			platformSlider.Foreground = brush;
		}

		public static void UpdateMaximumTrackColor(this MauiSlider platformSlider, ISlider slider, Brush? defaultMaximumTrackColor)
		{
			var brush = slider.MaximumTrackColor.IsDefault() ? defaultMaximumTrackColor : slider.MaximumTrackColor.ToPlatform();

			platformSlider.Resources["SliderTrackFill"] = brush;
			platformSlider.Resources["SliderTrackFillPointerOver"] = brush;
			platformSlider.Resources["SliderTrackFillPressed"] = brush;
			platformSlider.Resources["SliderTrackFillDisabled"] = brush;

			platformSlider.BorderBrush = brush;
		}

		public static void UpdateThumbColor(this MauiSlider nativeSlider, ISlider slider, Brush? defaultThumbColor)
		{
			var thumb = nativeSlider?.Thumb;

			if (thumb == null || slider?.ThumbColor == null || nativeSlider == null)
				return;

			nativeSlider.ThumbColorOver = slider.ThumbColor.ToPlatform();
			BrushHelpers.UpdateColor(slider.ThumbColor, ref defaultThumbColor,
				() => thumb.Background, brush => thumb.Background = brush);
    }
    
		public static async Task UpdateThumbImageSourceAsync(this MauiSlider nativeSlider, ISlider slider, IImageSourceServiceProvider? provider)
		{
			var thumbImageSource = slider.ThumbImageSource;

			if (thumbImageSource == null)
			{
				nativeSlider.ThumbImageSource = null;
				return;
			}

			if (provider != null && thumbImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var nativeThumbImageSource = await service.GetImageSourceAsync(thumbImageSource);

				nativeSlider.ThumbImageSource = nativeThumbImageSource?.Value;
			}
		}
	}
}