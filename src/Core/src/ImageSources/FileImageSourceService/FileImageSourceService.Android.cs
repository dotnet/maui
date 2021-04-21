using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
				return GetDrawableAsync(fileImageSource, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFileImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			var manager = Glide
				.With(context);

			var target = manager
				.Load(filename, context)
				.Submit();

			var result = await GlideImageSourceServiceResult.CreateAsync(target, manager, cancellationToken);

			return result;
		}
	}
}