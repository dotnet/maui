using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, ImageView>
	{

		protected override ImageView CreatePlatformView()
		{
			var imageView = new AppCompatImageView(Context);

			// Enable view bounds adjustment on measure.
			// This allows the ImageView's OnMeasure method to account for the image's intrinsic
			// aspect ratio during measurement, which gives us more useful values during constrained
			// measurement passes.
			imageView.SetAdjustViewBounds(true);

			return imageView;
		}

		protected override void ConnectHandler(ImageView platformView)
		{
			platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;
		}

		protected override void DisconnectHandler(ImageView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
			SourceLoader.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(image);
			handler.ToPlatform().UpdateOpacity(image);
		}

		public static void MapAspect(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			await handler
				.SourceLoader
				.UpdateImageSourceAsync();


			// This indicates that the image has finished loading
			// So, now if the attached event fires again then we need to see if Glide has cleared the image out
			handler.SourceLoader.CheckForImageLoadedOnAttached = true;

			// Because this resolves from a task we should validate that the
			// handler hasn't been disconnected
			if (handler.IsConnected())
			{
				handler.UpdateValue(nameof(IImage.IsAnimationPlaying));
			}
		}

		public override void PlatformArrange(Graphics.Rect frame)
		{
			if (PlatformView.GetScaleType() == ImageView.ScaleType.CenterCrop)
			{
				// If the image is center cropped (AspectFill), then the size of the image likely exceeds
				// the view size in some dimension. So we need to clip to the view's bounds.

				var (left, top, right, bottom) = PlatformView.Context!.ToPixels(frame);
				var clipRect = new Android.Graphics.Rect(0, 0, right - left, bottom - top);
				PlatformView.ClipBounds = clipRect;
			}
			else
			{
				PlatformView.ClipBounds = null;
			}

			base.PlatformArrange(frame);
		}

		internal static void OnPlatformViewAttachedToWindow(IImageHandler imageHandler)
		{

			// Glide will automatically clear out the image if the Fragment or Activity is destroyed
			// So we want to reload the image here if it's supposed to have an image
			if (imageHandler.SourceLoader.CheckForImageLoadedOnAttached &&
				imageHandler.PlatformView.Drawable is null &&
				imageHandler.VirtualView.Source is not null)
			{
				imageHandler.SourceLoader.CheckForImageLoadedOnAttached = false;
				imageHandler.UpdateValue(nameof(IImage.Source));
			}
		}

		void OnPlatformViewAttachedToWindow(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			if (sender is not View platformView)
			{
				return;
			}

			if (!this.IsConnected())
			{
				platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
				return;
			}

			OnPlatformViewAttachedToWindow(this);
		}

		partial class ImageImageSourcePartSetter
		{
			public override void SetImageSource(Drawable? platformImage)
			{
				if (Handler?.PlatformView is not ImageView image)
				{
					return;
				}

				image.SetImageDrawable(platformImage);
			}
		}
	}
}