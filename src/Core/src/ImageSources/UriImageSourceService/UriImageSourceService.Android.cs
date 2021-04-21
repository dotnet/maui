using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
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

			var manager = Glide
				.With(context);

			var builder = manager
				.Load(uri.OriginalString);

			if (!imageSource.CachingEnabled)
			{
				builder = builder
					.SetDiskCacheStrategy(DiskCacheStrategy.None)
					.SkipMemoryCache(true);
			}

			var target = builder
				.Submit();

			var result = await GlideImageSourceServiceResult.CreateAsync(target, manager, cancellationToken);

			return result;
		}
	}
}