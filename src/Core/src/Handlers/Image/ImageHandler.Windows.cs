using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		/// <inheritdoc/>
		protected override Image CreatePlatformView() => new Image();

		/// <inheritdoc/>
		protected override void DisconnectHandler(Image platformView)
		{
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
			PlatformView.Height = double.NaN;

			MapHeight(this, VirtualView);
			MapWidth(this, VirtualView);
			MapMaximumHeight(this, VirtualView);
			MapMaximumWidth(this, VirtualView);
		}

		/// <inheritdoc/>
		protected override void RemoveContainer()
		{
			base.RemoveContainer();

			MapHeight(this, VirtualView);
			MapWidth(this, VirtualView);
			MapMaximumHeight(this, VirtualView);
			MapMaximumWidth(this, VirtualView);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		public static void MapHeight(IImageHandler handler, IImage view)
		{
			// VerticalAlignment only works when the container's Height is set and the child's Height is Auto. The child's Height
			// is set to Auto when the container is introduced
			if (handler.ContainerView is FrameworkElement element)
			{
				element.Height = view.Height;
			}

			ViewHandler.MapHeight(handler, view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		public static void MapWidth(IImageHandler handler, IImage view)
		{
			if (handler.ContainerView is FrameworkElement element)
			{
				element.Width = view.Width;
			}

			ViewHandler.MapWidth(handler, view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MaximumHeight"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		public static void MapMaximumHeight(IImageHandler handler, IImage view)
		{
			if (handler.ContainerView is FrameworkElement element)
			{
				element.MaxHeight = view.MaximumHeight;
			}

			ViewHandler.MapMaximumHeight(handler, view);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.MaximumWidth"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="Image"/> instance.</param>
		public static void MapMaximumWidth(IImageHandler handler, IImage view)
		{
			if (handler.ContainerView is FrameworkElement element)
			{
				element.MaxWidth = view.MaximumWidth;
			}

			ViewHandler.MapMaximumWidth(handler, view);
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
