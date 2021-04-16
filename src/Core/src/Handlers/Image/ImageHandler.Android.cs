using System.Threading;
using System.Threading.Tasks;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, ImageView>
	{
		CancellationTokenSource? _sourceCancellation;

		protected override ImageView CreateNativeView() => new AppCompatImageView(Context);

		public static void MapAspect(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateAspect(image);
		}

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateIsAnimationPlaying(image);
		}

		public static async void MapSource(ImageHandler handler, IImage image) =>
			await MapSourceAsync(handler, image);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.NativeView == null)
				return;

			handler._sourceCancellation?.Cancel();
			handler._sourceCancellation = new CancellationTokenSource();

			await handler.NativeView.UpdateSourceAsync(image, handler.GetRequiredService<IImageSourceServiceProvider>(), handler._sourceCancellation.Token);

			handler._sourceCancellation = null;
		}
	}
}