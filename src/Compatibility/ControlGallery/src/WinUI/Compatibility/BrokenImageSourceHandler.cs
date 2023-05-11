using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Platform;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.WinUI
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<Microsoft.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = default)
		{
			throw new Exception("Fail");
		}
	}

	[System.Obsolete]
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
				await Maui.Controls.Application.Current.MainPage.DisplayAlert("Image Error 51173", $"The image failed to load, here's why: {ex.Message}", "OK");
			}
		}
	}
}