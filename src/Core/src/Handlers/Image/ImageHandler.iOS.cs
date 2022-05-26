#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreatePlatformView() => new MauiImageView();

		protected override void ConnectHandler(UIImageView platformView)
		{
			base.ConnectHandler(platformView);

			if (PlatformView is MauiImageView imageView)
				imageView.WindowChanged += OnWindowChanged;
		}

		protected override void DisconnectHandler(UIImageView platformView)
		{
			base.DisconnectHandler(platformView);

			if (PlatformView is MauiImageView imageView)
				imageView.WindowChanged -= OnWindowChanged;

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

		public static Task MapSourceAsync(IImageHandler handler, IImage image) =>
			handler.SourceLoader.UpdateImageSourceAsync();

		void OnSetImageSource(UIImage? obj) =>
			PlatformView.Image = obj;

		void OnWindowChanged(object? sender, EventArgs e)
		{
			if (SourceLoader.SourceManager.IsResolutionDependent)
				UpdateValue(nameof(IImage.Source));
		}
	}
}