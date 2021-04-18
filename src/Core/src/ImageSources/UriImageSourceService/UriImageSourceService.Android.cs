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
		public Task<Drawable?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource)
				return GetDrawableAsync(uriImageSource, context, cancellationToken);

			return Task.FromResult<Drawable?>(null);
		}

		public async Task<Drawable?> GetDrawableAsync(IUriImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var uri = imageSource.Uri;

			var builder = Glide
				.With(context)
				.Load(uri.OriginalString);

			if (!imageSource.CachingEnabled)
			{
				builder = builder
					.SetDiskCacheStrategy(DiskCacheStrategy.None)
					.SkipMemoryCache(true);
			}

			var target = builder
				.Submit();

			var drawable = await target.AsTask<Drawable>(cancellationToken);

			return drawable;
		}
	}
}