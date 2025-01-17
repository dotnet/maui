using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Extensions.DependencyInjection;
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
				var thumbImage = result?.Value;
				if (thumbImage != null)
				{
					// Standard iOS slider thumb size is approximately 28x28 points
					const float TARGET_SIZE = 28f;
					var thumbWidth = (float)thumbImage.Size.Width;
					var thumbHeight = (float)thumbImage.Size.Height;
					// Calculate scale factor based on the larger dimension
					float scaleFactor = TARGET_SIZE / Math.Max(thumbWidth, thumbHeight);
					// Create a new size maintaining aspect ratio
					var newSize = new CGSize(
						thumbWidth * scaleFactor,
						thumbHeight * scaleFactor
					);
					UIGraphics.BeginImageContextWithOptions(newSize, false, 0);
					thumbImage.Draw(new CGRect(0, 0, newSize.Width, newSize.Height));
					var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
					UIGraphics.EndImageContext();
					uiSlider.SetThumbImage(scaledImage, UIControlState.Normal);
				}
				else
				{
					uiSlider.SetThumbImage(null, UIControlState.Normal);
					uiSlider.UpdateThumbColor(slider);
				}
			}
			else
			{
				uiSlider.SetThumbImage(null, UIControlState.Normal);
				uiSlider.UpdateThumbColor(slider);
			}
		}
	}
}