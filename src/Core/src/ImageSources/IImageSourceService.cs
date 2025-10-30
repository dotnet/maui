#nullable enable
using System.Threading;
using System.Threading.Tasks;
#if ANDROID
using Android.Widget;
using Android.Graphics.Drawables;
#endif

namespace Microsoft.Maui
{
	public interface IImageSourceService
	{
#if ANDROID
		Task<IImageSourceServiceResult?> LoadDrawableAsync(
			IImageSource imageSource,
			ImageView imageView,
			CancellationToken cancellationToken = default);

		Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(
			IImageSource imageSource,
			global::Android.Content.Context context,
			CancellationToken cancellationToken = default);
#elif IOS
		Task<IImageSourceServiceResult<UIKit.UIImage>?> GetImageAsync(
			IImageSource imageSource,
			float scale = 1,
			CancellationToken cancellationToken = default);
#elif TIZEN || __TIZEN__
		Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(
			IImageSource imageSource,
			CancellationToken cancellationToken = default);
#elif WINDOWS
		Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>?> GetImageSourceAsync(
			IImageSource imageSource,
			float scale = 1,
			CancellationToken cancellationToken = default);
#endif
	}

	public interface IImageSourceService<in T> : IImageSourceService
		where T : IImageSource
	{
	}
}