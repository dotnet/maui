using Microsoft.Maui.Graphics;
using WImageBrush = Microsoft.UI.Xaml.Media.ImageBrush;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Minimum = stepper.Minimum;
		}

		public static void UpdateMaximum(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Maximum = stepper.Maximum;
		}

		public static void UpdateInterval(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Increment = stepper.Interval;
		}

		public static void UpdateValue(this MauiStepper platformStepper, IStepper stepper)
		{
			platformStepper.Value = stepper.Value;
		}

		public static void UpdateBackground(this MauiStepper platformStepper, IStepper stepper)
		{
			var background = stepper?.Background;
			if (background == null)
			{
				return;
			}

			if (background is ImageSourcePaint sourcePaint)
			{
				platformStepper.UpdateBackgroundImageSource(sourcePaint.ImageSource, stepper?.Handler);
			}
			else
			{
				platformStepper.ButtonBackground = background.ToPlatform();
			}
		}

		internal static void UpdateBackgroundImageSource(this MauiStepper platformStepper, IImageSource? imageSource, IElementHandler? handler)
		{
			var provider = handler?.GetRequiredService<IImageSourceServiceProvider>();
			UpdateBackgroundImageSourceAsync(platformStepper, imageSource, provider).FireAndForget(handler);
		}

		static async System.Threading.Tasks.Task UpdateBackgroundImageSourceAsync(MauiStepper platformStepper, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (provider is null || imageSource is null)
			{
				platformStepper.ButtonBackground = null;
				return;
			}

			var service = provider.GetRequiredImageSourceService(imageSource);
			var result = await service.GetImageSourceAsync(imageSource);
			var imageBrush = new WImageBrush { ImageSource = result?.Value };
			platformStepper.ButtonBackground = imageBrush;
		}
	}
}