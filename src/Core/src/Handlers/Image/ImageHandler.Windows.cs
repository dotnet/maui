using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
				// Only update container height if it actually changed to prevent layout instability during layout cycles
				if (container.Height != view.Height)
				{
					container.Height = view.Height;
				}
				
				if (handler.PlatformView.Height != Primitives.Dimension.Unset)
				{
					handler.PlatformView.Height = Primitives.Dimension.Unset;
				}
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
				// Only update container width if it actually changed to prevent layout instability during layout cycles
				if (container.Width != view.Width)
				{
					container.Width = view.Width;
				}
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
		public static Task MapSourceAsync(IImageHandler handler, IImage image) =>
			handler.SourceLoader.UpdateImageSourceAsync();

		void OnImageOpened(object sender, RoutedEventArgs e)
		{
			// Because this resolves from a task we should validate that the
			// handler hasn't been disconnected
			if (this.IsConnected())
				UpdateValue(nameof(IImage.IsAnimationPlaying));
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
