using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Controls.Issues;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms.ControlGallery.WindowsUniversal;

[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<Microsoft.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = default)
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
				await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Image Error 51173", $"The image failed to load, here's why: {ex.Message}", "OK");
			}
		}
	}
}