using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreImage;
using CoreText;
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

			var renderer = new UIGraphicsImageRenderer(imagesize, new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = scale,
			});

			return renderer.CreateImage((context) =>
			{
				if (OperatingSystem.IsIOSVersionAtLeast(26))
				{
					using var line = new CTLine(attString);
					var glyphBounds = line.GetBounds(CTLineBoundsOptions.UseGlyphPathBounds);
					if (glyphBounds.Width > 0 && glyphBounds.Height > 0)
					{
						// Center the visible ink, rather than the font's ascent and descent.
						var cgContext = context.CGContext;
						cgContext.TextMatrix = CGAffineTransform.MakeIdentity();
						cgContext.TranslateCTM(0, imagesize.Height);
						cgContext.ScaleCTM(1, -1);
						cgContext.TextPosition = new CGPoint(
							imagesize.Width / 2 - glyphBounds.GetMidX(),
							imagesize.Height / 2 - glyphBounds.GetMidY());
						line.Draw(cgContext);
						return;
					}
				}

				var ctx = new NSStringDrawingContext();

				var boundingRect = attString.GetBoundingRect(imagesize, 0, ctx);
				attString.DrawString(new CGRect(
					imagesize.Width / 2 - boundingRect.Size.Width / 2,
					imagesize.Height / 2 - boundingRect.Size.Height / 2,
					imagesize.Width,
					imagesize.Height));

				// Using UIRenderingMode.Automatic when the FontImageSource color is null (where 'Automatic' adapts based on the context and properly applies color) 
				// and UIRenderingMode.AlwaysOriginal when the FontImageSource color is specified ensures the given color is applied correctly
			}).ImageWithRenderingMode(imageSource.Color == null ? UIImageRenderingMode.Automatic : UIImageRenderingMode.AlwaysOriginal);
		}

		internal static UIImage? GetPlatformImage(this IFileImageSource imageSource)
		{
			var file = imageSource.File;

			// On iOS/MacCatalyst, MauiImage assets are flattened into the app bundle root.
			// UIImage.FromBundle() therefore requires just the base filename without path or extension.
			var bundleName = Path.GetFileNameWithoutExtension(file);
			return UIImage.FromBundle(bundleName) ?? UIImage.FromFile(file);
		}

		internal static CGImageSource? GetPlatformImageSource(this IFileImageSource imageSource, out int scale) =>
			CGImageSource.FromUrl(NSUrl.CreateFileUrl(imageSource.GetPlatformImagePath(out scale)));

		/// <summary>
		/// Gets the path of the file that is loaded for <paramref name="imageSource"/>: the scaled image in the app package, or the file itself.
		/// </summary>
		internal static string GetPlatformImagePath(this IFileImageSource imageSource, out int scale)
		{
			ArgumentNullException.ThrowIfNull(imageSource);

			var filename = imageSource.File;

			// search the bundle for the scaled image
			var bundle = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
			bundle = GetScaledFile(bundle, out scale);

			return File.Exists(bundle) ? bundle : filename;
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

		/// <summary>
		/// Creates the image like <see cref="GetPlatformImage(CGImageSource, int)"/>, but decoded at the size it is displayed at
		/// (see <paramref name="options"/>) when that is smaller than its full size. The image keeps its size in points, so that
		/// layout does not change; it only has fewer pixels.
		/// </summary>
		internal static UIImage GetPlatformImage(this CGImageSource cgImageSource, int scale, ImageDecodeOptions? options)
		{
			if (options is not null && RuntimeFeature.IsImageDownsamplingEnabled && TryGetDownsampledImage(cgImageSource, scale, options) is { } downsampled)
			{
				options.WasDownsampled = true;
				return downsampled;
			}

			return cgImageSource.GetPlatformImage(scale);
		}

		static double? GetDownsampleRatio(CGImageSource cgImageSource, ImageDecodeOptions options, out int width, out int height)
		{
			width = height = 0;

			// Center shows the image at its own size, so every pixel may be visible. Animations keep all their frames.
			if (options.Aspect == Aspect.Center || options.PixelWidth <= 0 || options.PixelHeight <= 0 ||
				cgImageSource.ImageCount != 1 || cgImageSource.IsAnimated())
			{
				return null;
			}

			var props = cgImageSource.GetProperties(0);
			if (props?.PixelWidth is not int pixelWidth || props.PixelHeight is not int pixelHeight || pixelWidth <= 0 || pixelHeight <= 0)
				return null;

			// The size of the image as it is shown, after applying its orientation.
			(width, height) = props.Orientation is CIImageOrientation.LeftTop or CIImageOrientation.RightTop or CIImageOrientation.RightBottom or CIImageOrientation.LeftBottom
				? (pixelHeight, pixelWidth)
				: (pixelWidth, pixelHeight);

			var widthRatio = options.PixelWidth / width;
			var heightRatio = options.PixelHeight / height;
			var ratio = options.Aspect == Aspect.AspectFit
				? Math.Min(widthRatio, heightRatio)
				: Math.Max(widthRatio, heightRatio);

			// Only decode smaller images, and only when it saves a meaningful amount of memory.
			return ratio < 0.8 ? ratio : null;
		}

		static UIImage? TryGetDownsampledImage(CGImageSource cgImageSource, int scale, ImageDecodeOptions options)
		{
			if (GetDownsampleRatio(cgImageSource, options, out var width, out var height) is not double ratio)
				return null;

			var maxPixelSize = (int)Math.Ceiling(Math.Max(width, height) * ratio);
			using var thumbnail = cgImageSource.CreateThumbnail(0, new CGImageThumbnailOptions
			{
				MaxPixelSize = maxPixelSize,
				CreateThumbnailFromImageAlways = true,
				CreateThumbnailWithTransform = true,
				ShouldCacheImmediately = true,
			});
			if (thumbnail is null)
				return null;

			// The thumbnail is already oriented. Keep the size in points of the full image: points = pixels / scale.
			var originalPoints = Math.Max(width, height) / (double)scale;
			var imageScale = Math.Max(thumbnail.Width, thumbnail.Height) / originalPoints;

			return new UIImage(thumbnail, (nfloat)imageScale, UIImageOrientation.Up);
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
