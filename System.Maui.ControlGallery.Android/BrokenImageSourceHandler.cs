using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls.Issues;
using System.Maui.Platform.Android;

[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
namespace System.Maui.ControlGallery.Android
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			throw new Exception("Fail");
		}
	}

#pragma warning disable 618
	public class _51173CustomImageRenderer : ImageRenderer
#pragma warning restore 618
	{
		protected override async Task TryUpdateBitmap(Image previous = null)
		{
			try
			{
				await UpdateBitmap(previous).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Image Error 51173", $"The image failed to load, here's why: {ex.Message}", "OK");
			}
		}
	}
}


