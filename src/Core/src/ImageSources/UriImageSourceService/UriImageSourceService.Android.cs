#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request.Target;
using Java.Interop;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override async Task<IImageSourceServiceResult<Drawable>?> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource)
			{
				try
				{
					var listener = new RequestBuilderExtensions.RequestCompleteListener();
					var glide = Glide.With(imageView.Context);
					var builder = glide
						.Load(uriImageSource.Uri.OriginalString)
						.AddListener(listener);

					if (!uriImageSource.CachingEnabled)
					{
						builder = builder
							.SetDiskCacheStrategy(DiskCacheStrategy.None)
							.SkipMemoryCache(true);
					}

					// Load into the image view
					var viewTarget = builder.Into(imageView);

					// Wait for the result from the listener
					var result = await listener.Result.ConfigureAwait(false);

					return new ImageSourceServiceResult(result, () => glide.Clear(viewTarget));
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return null;
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IUriImageSource)imageSource, context, cancellationToken);

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

				if (result == null)
					throw new InvalidOperationException($"Unable to load image URI '{uri}'.");

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", uri);
				throw;
			}
		}
	}
}