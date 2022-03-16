using System;
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

		public Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICustomImageSourceStub imageSourceStub)
				return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));

			var color = imageSourceStub.Color;

			var drawable = _cache.Get(color);

			imageView.SetImageDrawable(drawable);

			var result = new ImageSourceServiceResult(drawable is not null, () => _cache.Return(color));

			return Task.FromResult<IImageSourceServiceResult<bool>>(result);
		}

		public Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable> callback, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICustomImageSourceStub imageSourceStub)
				return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));

			var color = imageSourceStub.Color;

			var drawable = _cache.Get(color);

			callback?.Invoke(drawable);

			var result = new ImageSourceServiceResult(drawable is not null, () => _cache.Return(color));

			return Task.FromResult<IImageSourceServiceResult<bool>>(result);
		}
	}
}