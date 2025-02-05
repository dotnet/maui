using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreImage;
using Foundation;
using ImageIO;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using UIKit;

namespace Microsoft.Maui
{
	public static partial class ImageSourceExtensions
	{
		internal static UIImage? GetPlatformImage(this IFontImageSource imageSource, IFontManager fontManager, float scale)
		{
			var font = fontManager.GetFont(imageSource.Font);
			var color = (imageSource.Color ?? Colors.White).ToPlatform();
			var glyph = (NSString)imageSource.Glyph;

			if (string.IsNullOrWhiteSpace(imageSource.Glyph))
			{
				return null;
			}

			var attString = new NSAttributedString(glyph, font, color);
			var imagesize = glyph.GetSizeUsingAttributes(attString.GetUIKitAttributes(0, out _)!);

			if (imagesize.Width <= 0 || imagesize.Height <= 0)
			{
				return null;
			}

			UIGraphics.BeginImageContextWithOptions(imagesize, false, scale);
			var ctx = new NSStringDrawingContext();

			var boundingRect = attString.GetBoundingRect(imagesize, 0, ctx);
			attString.DrawString(new CGRect(
				imagesize.Width / 2 - boundingRect.Size.Width / 2,
				imagesize.Height / 2 - boundingRect.Size.Height / 2,
				imagesize.Width,
				imagesize.Height));

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return image.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
		}

		internal static UIImage? GetPlatformImage(this IFileImageSource imageSource)
		{
			var filename = imageSource.File;
			return UIImage.FromBundle(filename) ?? UIImage.FromFile(filename);
		}

		internal static CGImageSource? GetPlatformImageSource(this IFileImageSource imageSource, out int scale)
		{
			ArgumentNullException.ThrowIfNull(imageSource);

			var filename = imageSource.File;

			// search the bundle for the scaled image
			var bundle = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
			bundle = GetScaledFile(bundle, out scale);

			var url = File.Exists(bundle)
				? NSUrl.CreateFileUrl(bundle)
				: NSUrl.CreateFileUrl(filename);
			return CGImageSource.FromUrl(url);
		}

		internal static string GetScaledFile(string filename, out int scale)
		{
			const int MaxScale = 3; // max of 3 seems to be what Apple has gone up to
			const int MinScale = 2; // only 2 because 1 is "no scale" without any special logic

			var screenScale = (int)UIScreen.MainScreen.Scale;
			if (screenScale > 1)
			{
				var dir = Path.GetDirectoryName(filename);
				var name = Path.GetFileNameWithoutExtension(filename);
				var ext = Path.GetExtension(filename);

				var loopMax = Math.Min(MaxScale, screenScale);
				for (var s = loopMax; s >= MinScale; s--)
				{
					var newName = $"{name}@{(int)s}x{ext}";
					var scaled = dir is null
						? newName
						: Path.Combine(dir, newName);

					if (File.Exists(scaled))
					{
						scale = s;
						return scaled;
					}
				}
			}

			scale = 1;
			return filename;
		}

		internal static async Task<CGImageSource?> GetPlatformImageSourceAsync(this IStreamImageSource imageSource, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(imageSource);

			var stream = await imageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				throw new ArgumentException("Unable to load image stream.");

			return stream.GetPlatformImageSource();
		}

		internal static CGImageSource? GetPlatformImageSource(this Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);

			var data = NSData.FromStream(stream);
			if (data is null)
				throw new ArgumentException("Stream contained no data.", nameof(stream));

			return data.GetPlatformImageSource();
		}

		internal static CGImageSource? GetPlatformImageSource(this NSData data)
		{
			ArgumentNullException.ThrowIfNull(data);

			return CGImageSource.FromData(data);
		}

		internal static UIImage GetPlatformImage(this CGImageSource cgImageSource, int scale = 1)
		{
			ArgumentNullException.ThrowIfNull(cgImageSource);

			if (cgImageSource.ImageCount == 0)
				throw new InvalidOperationException("CGImageSource does not contain any images.");

			UIImage image;

			if (cgImageSource.IsAnimated())
			{
				var animated = ImageAnimationHelper.Create(cgImageSource, scale);
				if (animated is null)
					throw new InvalidOperationException("Unable to create animation from CGImageSource.");

				image = animated;
			}
			else
			{
				using var cgimage = cgImageSource.CreateImage(0, new() { ShouldCache = false });
				if (cgimage is null)
					throw new InvalidOperationException("Unable to create CGImage from CGImageSource.");

				image = new UIImage(cgimage, scale, ToUIImageOrientation(cgImageSource));
			}

			return image;
		}

		static UIImageOrientation ToUIImageOrientation(CGImageSource cgImageSource)
		{
			var props = cgImageSource.GetProperties(0);
			if (props is null || props.Orientation is null)
				return UIImageOrientation.Up;

			return ToUIImageOrientation(props.Orientation.Value);
		}

		static UIImageOrientation ToUIImageOrientation(CIImageOrientation cgOrient) => cgOrient switch
		{
			CIImageOrientation.TopLeft => UIImageOrientation.Up,
			CIImageOrientation.TopRight => UIImageOrientation.UpMirrored,
			CIImageOrientation.BottomRight => UIImageOrientation.Down,
			CIImageOrientation.BottomLeft => UIImageOrientation.DownMirrored,
			CIImageOrientation.LeftTop => UIImageOrientation.LeftMirrored,
			CIImageOrientation.RightTop => UIImageOrientation.Right,
			CIImageOrientation.RightBottom => UIImageOrientation.RightMirrored,
			CIImageOrientation.LeftBottom => UIImageOrientation.Left,
			_ => throw new ArgumentOutOfRangeException(nameof(cgOrient)),
		};
	}
}
