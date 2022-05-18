#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<Image>?> GetImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFontImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<Image>?> GetImageAsync(IFontImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				//TODO : Fix me correctly later.
				var isLoadComplated = await image.LoadAsync(string.Empty, cancellationToken);

				if (!isLoadComplated)
				{
					throw new InvalidOperationException("Unable to load image file.");
				}

				var result = new ImageSourceServiceResult(image);
				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", imageSource.Glyph);
				throw;
			}
		}
	}
}