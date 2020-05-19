using System;
using System.Threading;
using System.Threading.Tasks;
using System.Maui.Controls.Issues;
using WImageSource = global::Windows.UI.Xaml.Media.ImageSource;

using System.Maui.Platform.UWP;
using System.Maui.ControlGallery.WindowsUniversal;

[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<WImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			throw new Exception("Fail");
		}
	}

	public class _51173CustomImageRenderer : ImageRenderer
	{
		protected override async Task TryUpdateSource()
		{
			try
			{
				await UpdateSource().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await System.Maui.Application.Current.MainPage.DisplayAlert("Image Error 51173", $"The image failed to load, here's why: {ex.Message}", "OK");
			}
		}
	}
}