#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IFileImageSource)imageSource, context, cancellationToken);

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFileImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			try
			{
				var result = await Glide
					.With(context)
					.Load(filename, context)
					.SubmitAsync(context, cancellationToken)
					.ConfigureAwait(false);

				if (result == null)
					throw new InvalidOperationException("Unable to load image file.");

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);
				throw;
			}
		}
	}
}