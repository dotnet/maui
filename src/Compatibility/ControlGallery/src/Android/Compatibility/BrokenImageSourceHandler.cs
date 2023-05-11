using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: ExportRenderer(typeof(_51173Image), typeof(_51173CustomImageRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
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
		public _51173CustomImageRenderer(Context context) : base(context)
		{
		}

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


