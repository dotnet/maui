using System.Threading.Tasks;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, ImageView>
	{
		protected override ImageView CreateNativeView() => new AppCompatImageView(Context);

		protected override void DisconnectHandler(ImageView nativeView)
		{
			base.DisconnectHandler(nativeView);

			_sourceManager.Reset();
		}

		public static void MapAspect(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateIsAnimationPlaying(image);

		public static async void MapSource(ImageHandler handler, IImage image) =>
			await MapSourceAsync(handler, image);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.NativeView == null)
				return;

			var token = handler._sourceManager.BeginLoad();

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var result = await handler.NativeView.UpdateSourceAsync(image, provider, token);

			handler._sourceManager.CompleteLoad(result);
		}
	}
}