#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
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
				var image = imageSource.GetPlatformImage(FontManager, scale);

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
	}
}