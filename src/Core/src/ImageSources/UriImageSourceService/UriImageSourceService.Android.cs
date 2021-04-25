using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource)
				return GetDrawableAsync(uriImageSource, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IUriImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var uri = imageSource.Uri;

			try
			{
				var builder = Glide
					.With(context)
					.Load(uri.OriginalString);

				if (!imageSource.CachingEnabled)
				{
					builder = builder
						.SetDiskCacheStrategy(DiskCacheStrategy.None)
						.SkipMemoryCache(true);
				}

				var result = await builder
					.SubmitAsync(context, cancellationToken)
					.ConfigureAwait(false);

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", uri);
				return null;
			}
		}
	}
}