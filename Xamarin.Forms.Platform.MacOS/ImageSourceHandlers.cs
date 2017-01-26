using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<NSImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken),
			float scale = 1);
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<NSImage> LoadImageAsync(ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			NSImage image = null;
			var filesource = imagesource as FileImageSource;
			var file = filesource?.File;
			if (!string.IsNullOrEmpty(file))
				image = File.Exists(file) ? new NSImage(file) : null;
			return Task.FromResult(image);
		}
	}

	public sealed class StreamImagesourceHandler : IImageSourceHandler
	{
		public async Task<NSImage> LoadImageAsync(ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			NSImage image = null;
			var streamsource = imagesource as StreamImageSource;
			if (streamsource?.Stream == null) return null;
			using (
				var streamImage = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
			{
				if (streamImage != null)
					image = NSImage.FromStream(streamImage);
			}
			return image;
		}
	}

	public sealed class ImageLoaderSourceHandler : IImageSourceHandler
	{
		public async Task<NSImage> LoadImageAsync(ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			NSImage image = null;
			var imageLoader = imagesource as UriImageSource;
			if (imageLoader != null && imageLoader.Uri != null)
			{
				using (var streamImage = await imageLoader.GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
						image = NSImage.FromStream(streamImage);
				}
			}
			return image;
		}
	}
}