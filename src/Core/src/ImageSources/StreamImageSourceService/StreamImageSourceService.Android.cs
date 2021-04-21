using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IStreamImageSource streamImageSource)
				return GetDrawableAsync(streamImageSource, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IStreamImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var stream = await imageSource.Stream.Invoke(cancellationToken);

			// We can use the .NET stream directly because we register the InputStreamModelLoader.
			// There are 2 alternatives:
			//  - Load the bitmap manually and pass that along, but then we do not get the decoding features.
			//  - Copy the stream into a byte array and that is double memory usage - especially for large streams.
			var inputStream = new InputStreamAdapter(stream);

			var manager = Glide
				.With(context);

			var target = manager
				.Load(inputStream)
				.SetDiskCacheStrategy(DiskCacheStrategy.None)
				.Submit();

			var result = await GlideImageSourceServiceResult.CreateAsync(target, manager, cancellationToken);

			return result;
		}
	}
}