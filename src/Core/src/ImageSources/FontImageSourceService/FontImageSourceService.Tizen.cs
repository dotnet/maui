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
		public override Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			LoadImageAsync((IFontImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IFontImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				//TODO : Fix me correctly later.
				var isLoadComplate = await image.LoadAsync(string.Empty, cancellationToken);

				if (!isLoadComplate)
				{
					throw new InvalidOperationException("Unable to load image file.");
				}

				var result = new ImageSourceServiceResult(isLoadComplate);

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