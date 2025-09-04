#nullable enable
using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;
using Microsoft.Maui.Diagnostics;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		protected override Image CreatePlatformView() => new Image();

		protected override void DisconnectHandler(Image platformView)
		{
			base.DisconnectHandler(platformView);
			SourceLoader.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			base.NeedsContainer;

		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(image);
		}

		public static void MapAspect(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			using var instrumentation =
				DiagnosticInstrumentation.StartImageLoading(handler.VirtualView, GetSourceType(image.Source));

			await handler.SourceLoader.UpdateImageSourceAsync();
		}

		partial class ImageImageSourcePartSetter
		{
			public override void SetImageSource(MauiImageSource? platformImage)
			{
				if (Handler?.PlatformView is not Image image)
					return;

				if (platformImage is null)
					return;

				image.ResourceUrl = platformImage.ResourceUrl;
			}
		}
	}
}
