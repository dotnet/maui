using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public static void UpdateMinimum(this UISlider uiSlider, ISlider slider)
		{
			uiSlider.MinValue = (float)slider.Minimum;
		}

		public static void UpdateMaximum(this UISlider uiSlider, ISlider slider)
		{
			uiSlider.MaxValue = (float)slider.Maximum;
		}

		public static void UpdateValue(this UISlider uiSlider, ISlider slider)
		{
			if ((float)slider.Value != uiSlider.Value)
				uiSlider.Value = (float)slider.Value;
		}

		public static void UpdateMinimumTrackColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.MinimumTrackColor != null)
				uiSlider.MinimumTrackTintColor = slider.MinimumTrackColor.ToPlatform();
		}

		public static void UpdateMaximumTrackColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.MaximumTrackColor != null)
				uiSlider.MaximumTrackTintColor = slider.MaximumTrackColor.ToPlatform();
		}

		public static void UpdateThumbColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.ThumbColor != null)
				uiSlider.ThumbTintColor = slider.ThumbColor.ToPlatform();
		}


		public static async Task UpdateThumbImageSourceAsync(this UISlider uiSlider, ISlider slider, IImageSourceServiceProvider provider)
		{
			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var scale = uiSlider.GetDisplayDensity();
				var result = await service.GetImageAsync(thumbImageSource, scale);
				UIImage? thumbImage = null;

				var thumbImageSize = result?.Value.Size ?? CGSize.Empty;
				const float TARGET_SIZE = 28f;
				var buffer = 0.1;
				if (thumbImageSize.Height - TARGET_SIZE > buffer || thumbImageSize.Width - TARGET_SIZE > buffer)
				{
					thumbImage = result?.Value?.ResizeImageSource(TARGET_SIZE, TARGET_SIZE, thumbImageSize);
				}
				else
				{
					thumbImage = result?.Value;
				}
				uiSlider.SetThumbImage(thumbImage, UIControlState.Normal);
			}
			else
			{
				uiSlider.SetThumbImage(null, UIControlState.Normal);
				uiSlider.UpdateThumbColor(slider);
			}
		}
	}
}