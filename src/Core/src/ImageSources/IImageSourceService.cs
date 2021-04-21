using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IImageSourceService
	{
#if __ANDROID__
		Task<IImageSourceServiceResult<Android.Graphics.Drawables.Drawable>?> GetDrawableAsync(
			IImageSource imageSource,
			Android.Content.Context context,
			CancellationToken cancellationToken = default);
#endif
	}

	public interface IImageSourceService<T> : IImageSourceService
		where T : IImageSource
	{
	}

	public interface IImageSourceServiceResult<T> : IDisposable
	{
		T Value { get; }
	}
}