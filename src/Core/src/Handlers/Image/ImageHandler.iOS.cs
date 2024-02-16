using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreatePlatformView() => new MauiImageView(this);

		protected override void DisconnectHandler(UIImageView platformView)
		{
			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(image);
		}

		public static void MapAspect(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(IImageHandler handler, IImage image) =>
			await handler.SourceLoader.UpdateImageSourceAsync();

		public void OnWindowChanged()
		{
			if (SourceLoader.SourceManager.RequiresReload(PlatformView))
				UpdateValue(nameof(IImage.Source));
		}

		partial class ImageImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler?.PlatformView is not UIImageView imageView)
					return;

				if (platformImage?.Images is not null)
				{
					imageView.Image = platformImage.Images[0];

					imageView.AnimationImages = platformImage.Images;
					imageView.AnimationDuration = platformImage.Duration;
				}
				else
				{
					imageView.AnimationImages = null;
					imageView.AnimationDuration = 0.0;

					imageView.Image = platformImage;
				}

				Handler?.UpdateValue(nameof(IImage.IsAnimationPlaying));

				if (Handler?.VirtualView is IImage image && image.Source is IStreamImageSource)
					imageView.InvalidateMeasure(image);
			}
		}
	}
}