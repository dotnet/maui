﻿using System.Threading.Tasks;
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
			{
				uiSlider.Value = (float)slider.Value;
			}
		}

		public static void UpdateMinimumTrackColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.MinimumTrackColor is not null)
			{
				uiSlider.MinimumTrackTintColor = slider.MinimumTrackColor.ToPlatform();
			}
		}

		public static void UpdateMaximumTrackColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.MaximumTrackColor is not null)
			{
				uiSlider.MaximumTrackTintColor = slider.MaximumTrackColor.ToPlatform();
			}
		}

		public static void UpdateThumbColor(this UISlider uiSlider, ISlider slider)
		{
			if (slider.ThumbColor is not null)
			{
				uiSlider.ThumbTintColor = slider.ThumbColor.ToPlatform();
			}
		}

		public static async Task UpdateThumbImageSourceAsync(this UISlider uiSlider, ISlider slider, IImageSourceServiceProvider provider)
		{
			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource is not null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var scale = uiSlider.GetDisplayDensity();
				var result = await service.GetImageAsync(thumbImageSource, scale);
				var thumbImageSize = result?.Value.Size ?? CGSize.Empty;
				var defaultThumbSize = CalculateDefaultThumbSize(uiSlider);

				UIImage? thumbImage;
				if (thumbImageSize.IsEmpty)
				{
					thumbImage = result?.Value;
				}
				else
				{
					// Resize the image if the size is valid
					thumbImage = result?.Value?.ResizeImageSource(defaultThumbSize.Width, defaultThumbSize.Height, thumbImageSize);
				}

				uiSlider.SetThumbImage(thumbImage, UIControlState.Normal);
			}
			else
			{
				uiSlider.SetThumbImage(null, UIControlState.Normal);
				uiSlider.UpdateThumbColor(slider);
			}
		}

		static CGSize CalculateDefaultThumbSize(UISlider uiSlider)
		{
			var trackRect = uiSlider.TrackRectForBounds(uiSlider.Bounds);
			var thumbRect = uiSlider.ThumbRectForBounds(uiSlider.Bounds, trackRect, 0);
			return thumbRect.Size;
		}
	}
}