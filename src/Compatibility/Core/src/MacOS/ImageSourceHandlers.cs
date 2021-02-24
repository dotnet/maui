using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using CoreText;
using Foundation;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
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
				image = File.Exists(file) ? new NSImage(file) : NSImage.ImageNamed(file);

			if (image == null)
			{
				Log.Warning(nameof(FileImageSourceHandler), "Could not find image: {0}", imagesource);
			}

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

	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		readonly Color _defaultColor = Color.White;

		public Task<NSImage> LoadImageAsync(
			ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken),
			float scale = 1f)
		{ 
			NSImage image = null;
			var fontsource = imagesource as FontImageSource;
			if (fontsource != null)
			{
				var font = NSFont.FromFontName(fontsource.FontFamily ?? string.Empty, (float)fontsource.Size) ??
					NSFont.SystemFontOfSize((float)fontsource.Size);
				var iconcolor = fontsource.Color.IsDefault ? _defaultColor : fontsource.Color;
				var attString = new NSAttributedString(fontsource.Glyph, font: font, foregroundColor: iconcolor.ToNSColor());
				var imagesize = ((NSString)fontsource.Glyph).StringSize(attString.GetAttributes(0, out _));

				using (var context = new CGBitmapContext(IntPtr.Zero, (nint)imagesize.Width, (nint)imagesize.Height, 8, (nint)imagesize.Width * 4, NSColorSpace.GenericRGBColorSpace.ColorSpace, CGImageAlphaInfo.PremultipliedFirst))
				{
					using (var ctline = new CTLine(attString))
					{
						ctline.Draw(context);
					}

					using (var cgImage = context.ToImage())
					{
						image = new NSImage(cgImage, imagesize);
					}
				}
			}

			return Task.FromResult(image);
		}
	}
}