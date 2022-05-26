using System.Threading.Tasks;
using Tizen.NUI.Components;

namespace Microsoft.Maui.Platform
{
	public static class SliderExtensions
	{
		public static void UpdateMinimum(this Slider platformSlider, ISlider slider)
		{
			platformSlider.MinValue = (float)slider.Minimum;
		}

		public static void UpdateMaximum(this Slider platformSlider, ISlider slider)
		{
			platformSlider.MaxValue = (float)slider.Maximum;
		}

		public static void UpdateValue(this Slider platformSlider, ISlider slider)
		{
			platformSlider.CurrentValue = (float)slider.Value;
		}

		public static void UpdateMinimumTrackColor(this Slider platformSlider, ISlider slider)
		{
			platformSlider.SlidedTrackColor = slider.MinimumTrackColor.ToNUIColor();
		}

		public static void UpdateMaximumTrackColor(this Slider platformSlider, ISlider slider)
		{
			platformSlider.BgTrackColor = slider.MaximumTrackColor.ToNUIColor();
		}

		public static void UpdateThumbColor(this Slider platformSlider, ISlider slider)
		{
			platformSlider.ThumbColor = slider.ThumbColor.ToNUIColor();
		}

		public static async Task UpdateThumbImageSourceAsync(this Slider platformSlider, ISlider slider, IImageSourceServiceProvider provider)
		{
			var thumbImageSource = slider.ThumbImageSource;
			if (thumbImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(thumbImageSource);
				var result = await service.GetImageAsync(thumbImageSource);

				if (result != null)
				{
					platformSlider.ThumbImageUrl = result.Value.ResourceUrl;
				}
			}
		}
	}
}