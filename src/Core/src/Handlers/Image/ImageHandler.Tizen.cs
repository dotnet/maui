#nullable enable
using System;
using System.Threading.Tasks;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		protected override Image CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new ImageEx(NativeParent);
		}

		protected override void DisconnectHandler(Image nativeView)
		{
			base.DisconnectHandler(nativeView);
			_sourceManager.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			base.NeedsContainer;

		public static void MapBackground(ImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.WrappedNativeView?.UpdateBackground(image);
		}

		public static void MapAspect(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(ImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

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

	// TODO : Will be removed. Use ImageEx temporaily before Tizen.UIExtension.Image fixing.
	class ImageEx : Image, IMeasurable
	{
		public ImageEx(ElmSharp.EvasObject parent) : base(parent) { }

		Size IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			var imageSize = ObjectSize;
			var size = new Size()
			{
				Width = imageSize.Width,
				Height = imageSize.Height,
			};

			if (0 != availableWidth && 0 != availableHeight
				&& (imageSize.Width > availableWidth || imageSize.Height > availableHeight))
			{
				// when available size is limited and insufficient for the image ...
				double imageRatio = imageSize.Width / imageSize.Height;
				double availableRatio = availableWidth / availableHeight;
				// depending on the relation between availableRatio and imageRatio, copy the availableWidth or availableHeight
				// and calculate the size which preserves the image ratio, but does not exceed the available size
				size.Width = availableRatio > imageRatio ? imageSize.Width * availableHeight / imageSize.Height : availableWidth;
				size.Height = availableRatio > imageRatio ? availableHeight : imageSize.Height * availableWidth / imageSize.Width;
			}

			return size;
		}
	}
}