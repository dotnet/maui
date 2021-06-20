using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class CustomImageSourceServiceStub : IImageSourceService<ICustomImageSourceStub>
	{
		readonly CustomImageCacheStub _cache;

		public CustomImageSourceServiceStub(CustomImageCacheStub cache)
		{
			_cache = cache;
		}

		public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is ICustomImageSourceStub counted)
				return GetDrawableAsync(counted, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>>(null);
		}

		public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(ICustomImageSourceStub imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var color = imageSource.Color;

			var drawable = _cache.Get(color);

			var result = new ImageSourceServiceResult(drawable, () => _cache.Return(color));

			return Task.FromResult<IImageSourceServiceResult<Drawable>>(result);
		}
	}
}