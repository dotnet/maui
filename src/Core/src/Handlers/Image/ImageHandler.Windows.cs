using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, WImage>
	{
		/// <inheritdoc/>
		protected override WImage CreatePlatformView() => new WImage();

		/// <inheritdoc/>
		protected override void ConnectHandler(WImage platformView)
		{
			platformView.ImageOpened += OnImageOpened;

			base.ConnectHandler(platformView);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(WImage platformView)
		{
			platformView.ImageOpened -= OnImageOpened;

			base.DisconnectHandler(platformView);
			SourceLoader.Reset();
		}

		/// <inheritdoc/>
		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Aspect == Aspect.AspectFill ||
			base.NeedsContainer;

		/// <inheritdoc/>
		protected override void SetupContainer()
		{
			base.SetupContainer();

			// VerticalAlignment only works when the child's Height is Auto
			PlatformView.Height = Primitives.Dimension.Unset;

			UpdateValue(nameof(IView.Height));
			UpdateValue(nameof(IView.Width));
		}

		/// <inheritdoc/>
		protected override void RemoveContainer()
		{
			base.RemoveContainer();

			UpdateValue(nameof(IView.Height));
			UpdateValue(nameof(IView.Width));
		}

		/// <inheritdoc/>
		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			// Compute a possible size without mutating platform properties during measure.
			var possibleSize = base.GetDesiredSize(widthConstraint, heightConstraint);

			// For AspectFit + non-Fill alignment, don't exceed intrinsic bitmap size so alignment works.
			if (VirtualView.Aspect == Aspect.AspectFit
				&& VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill
				&& VirtualView.VerticalLayoutAlignment != Primitives.LayoutAlignment.Fill)
			{
				var imageSize = GetImageSize();
				if (imageSize.Width > 0 && imageSize.Height > 0)
				{
					return new Graphics.Size(
						Math.Min(possibleSize.Width, imageSize.Width),
						Math.Min(possibleSize.Height, imageSize.Height));
				}
			}

			return possibleSize;
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		internal static void MapHeight(IImageHandler handler, IImage view)
		{
			// VerticalAlignment only works when the container's Height is set and the child's Height is Auto. The child's Height
			// is set to Auto when the container is introduced.
			if (handler.ContainerView is FrameworkElement container)
			{
				container.Height = view.Height;
				handler.PlatformView.Height = Primitives.Dimension.Unset;
			}
			else
			{
				ViewHandler.MapHeight(handler, view);
			}
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		internal static void MapWidth(IImageHandler handler, IImage view)
		{
			if (handler.ContainerView is FrameworkElement container)
			{
				container.Width = view.Width;
			}
			else
			{
				ViewHandler.MapWidth(handler, view);
			}
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="image">The associated <see cref="Image"/> instance.</param>
		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform().UpdateBackground(image);
		}

		/// <summary>
		/// Maps the abstract <see cref="IImage.Aspect"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="image">The associated <see cref="Image"/> instance.</param>
		public static void MapAspect(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.PlatformView?.UpdateAspect(image);
			// Aspect changes may affect whether we cap to intrinsic size
			if (handler is ImageHandler ih)
				ih.UpdatePlatformMaxConstraints();
		}

		/// <summary>
		/// Maps the abstract <see cref="IImageSourcePart.IsAnimationPlaying"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="image">The associated <see cref="Image"/> instance.</param>
		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateIsAnimationPlaying(image);

		/// <summary>
		/// Maps the abstract <see cref="IImageSourcePart.Source"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="image">The associated <see cref="Image"/> instance.</param>
		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		/// <summary>
		/// Maps the abstract <see cref="IImageSourcePart.Source"/> property to the platform-specific implementations as an asynchronous operation.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="image">The associated <see cref="Image"/> instance.</param>
		public static Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			// Reset platform caps so we don't keep stale values between sources
			if (handler is ImageHandler ih && ih.PlatformView is not null)
			{
				ih.PlatformView.MaxWidth = double.PositiveInfinity;
				ih.PlatformView.MaxHeight = double.PositiveInfinity;
			}

			return handler.SourceLoader.UpdateImageSourceAsync();
		}

		void OnImageOpened(object sender, RoutedEventArgs e)
		{
			// Because this resolves from a task we should validate that the
			// handler hasn't been disconnected
			if (this.IsConnected())
			{
				UpdateValue(nameof(IImage.IsAnimationPlaying));
				// Apply platform constraints when the decoded size is available
				UpdatePlatformMaxConstraints();
			}
		}

		/// <summary>
		/// Updates platform MaxWidth/MaxHeight based on current aspect/alignment and decoded image size.
		/// Avoids doing this during GetDesiredSize to prevent side effects across layout passes.
		/// </summary>
		private void UpdatePlatformMaxConstraints()
		{
			if (PlatformView is null || VirtualView is null)
				return;

			bool isAspectFitNonFill = VirtualView.Aspect == Aspect.AspectFit
				&& VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill
				&& VirtualView.VerticalLayoutAlignment != Primitives.LayoutAlignment.Fill;

			if (isAspectFitNonFill)
			{
				var sz = GetImageSize();
				if (sz.Width > 0 && sz.Height > 0)
				{
					PlatformView.MaxWidth = sz.Width;
					PlatformView.MaxHeight = sz.Height;
					return;
				}
			}

			// Otherwise mirror the view's declared maximums
			PlatformView.MaxWidth = VirtualView.MaximumWidth;
			PlatformView.MaxHeight = VirtualView.MaximumHeight;
		}

		private Graphics.Size GetImageSize()
		{
			if (PlatformView.Source is BitmapSource bitmap)
			{
				// BitmapSource may not have PixelWidth/PixelHeight set until image is loaded
				if (bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0)
				{
					return new Graphics.Size(bitmap.PixelWidth, bitmap.PixelHeight);
				}
				// If not available, return zero
			}
			return Graphics.Size.Zero;
		}

		partial class ImageImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not WImage image)
					return;

				image.Source = platformImage;
			}
		}
	}
}
