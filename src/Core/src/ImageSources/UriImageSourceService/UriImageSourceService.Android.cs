using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;

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

			var target = Glide
				.With(context)
				.Load(uri.OriginalString)
				.Submit();

			var drawable = await target.AsTask<Drawable>(cancellationToken);

			return drawable;
		}
	}
}