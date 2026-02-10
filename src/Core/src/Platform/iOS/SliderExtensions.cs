using System.Threading.Tasks;
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
			if (slider.ThumbImageSource is not null && slider.Handler is not null)
			{
				var provider = slider.Handler.GetRequiredService<IImageSourceServiceProvider>();
				uiSlider.UpdateThumbImageSourceAsync(slider, provider).FireAndForget();
			}
			else if (slider.ThumbColor is not null)
			{
				uiSlider.ThumbTintColor = slider.ThumbColor.ToPlatform();
			}
		}

		public static async Task UpdateThumbImageSourceAsync(this UISlider uiSlider, ISlider slider, IImageSourceServiceProvider provider)
		{
			var thumbImageSource = slider.ThumbImageSource;

			if (thumbImageSource != null)
			{
				// Clear the thumb color if we have a thumb image, so that slider doesn't clear image when sliding
				uiSlider.ThumbTintColor = null;
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var scale = uiSlider.GetDisplayDensity();
				var result = await service.GetImageAsync(thumbImageSource, scale);
				var thumbImage = result?.Value;

				if (thumbImage is not null && slider.ThumbColor is not null)
				{
					thumbImage = thumbImage.ApplyTintColor(slider.ThumbColor.ToPlatform());
				}

				uiSlider.SetThumbImage(thumbImage, UIControlState.Normal);

				// On iOS 26+, SetThumbImage() no longer triggers a layout pass that recalculates
				// the thumb position at runtime. Explicitly call SetNeedsLayout() to restore this.
				if (System.OperatingSystem.IsIOSVersionAtLeast(26))
				{
					uiSlider.SetNeedsLayout();
				}
			}
			else
			{
				uiSlider.SetThumbImage(null, UIControlState.Normal);

				// On iOS 26+, SetThumbImage() no longer triggers a layout pass that recalculates
				// the thumb position at runtime. Explicitly call SetNeedsLayout() to restore this.
				if (System.OperatingSystem.IsIOSVersionAtLeast(26))
				{
					uiSlider.SetNeedsLayout();
				}

				uiSlider.UpdateThumbColor(slider);
			}
		}
	}
}