using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public Task<Drawable?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
				return GetDrawableAsync(fileImageSource, context, cancellationToken);

			return Task.FromResult<Drawable?>(null);
		}

		public async Task<Drawable?> GetDrawableAsync(IFileImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			var target = Glide
				.With(context)
				.Load(filename, context)
				.Submit();

			var drawable = await target.AsTask<Drawable>(cancellationToken);

			return drawable;
		}
	}
}