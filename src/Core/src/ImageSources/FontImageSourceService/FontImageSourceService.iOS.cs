#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFontImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IFontImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			try
			{
				// TODO: use a cached way
				var image = RenderImage(imageSource, scale);

				if (image == null)
					throw new InvalidOperationException("Unable to generate font image.");

				var result = new ImageSourceServiceResult(image, true, () => image.Dispose());

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", imageSource.Glyph);
				throw;
			}
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);

		internal UIImage RenderImage(IFontImageSource imageSource, float scale)
		{
			var font = FontManager.GetFont(imageSource.Font);
			var color = (imageSource.Color ?? Colors.White).ToNative();
			var glyph = (NSString)imageSource.Glyph;

			var attString = new NSAttributedString(glyph, font, color);
			var imagesize = glyph.GetSizeUsingAttributes(attString.GetUIKitAttributes(0, out _));

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

			return image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}
	}
}