using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Issues;
using System.Maui.Platform.iOS;

[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
namespace System.Maui.ControlGallery.iOS
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = new CancellationToken(),
			float scale = 1)
		{
			throw new Exception("Fail");
		}
	}

	public class _51173CustomImageRenderer : ImageRenderer
	{
		protected override async Task TrySetImage(Image previous = null)
		{
			try
			{
				await SetImage(previous).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await System.Maui.Application.Current.MainPage.DisplayAlert("Image Error 51173", $"The image failed to load, here's why: {ex.Message}", "OK");
			}
		}
	}
}
