using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using CAspect = Tizen.UIExtensions.Common.Aspect;
using NImage = Tizen.UIExtensions.NUI.Image;
using NUIImage = Tizen.NUI.BaseComponents.ImageView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ImageRenderer : ViewRenderer<Image, NImage>
	{
		public ImageRenderer()
		{
			RegisterPropertyHandler(Image.SourceProperty, UpdateSource);
			RegisterPropertyHandler(Image.AspectProperty, UpdateAspect);
			RegisterPropertyHandler(Image.IsAnimationPlayingProperty, UpdateIsAnimationPlaying);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				SetNativeControl(new NImage());
			}
			base.OnElementChanged(e);
		}

		async void UpdateSource(bool initialize)
		{
			if (initialize && Element.Source == default(ImageSource))
				return;

			ImageSource source = Element.Source;
			((IImageController)Element).SetIsLoading(true);

			if (Control != null)
			{
				bool success = await Control.LoadFromImageSourceAsync(source);

				if (!IsDisposed && success)
				{
					((IVisualElementController)Element).PlatformSizeChanged();
					UpdateAfterLoading(initialize);
				}
			}

			if (!IsDisposed)
				((IImageController)Element).SetIsLoading(false);
		}

		protected virtual void UpdateAfterLoading(bool initialize)
		{
		}

		void UpdateAspect()
		{
			Control.Aspect = (CAspect)Element.Aspect;
		}

		void UpdateIsAnimationPlaying(bool initialize)
		{
			// Default behavior of animation is true on NUI
			if (initialize && Element.IsAnimationPlaying)
				return;
			Control.SetIsAnimationPlaying(Element.IsAnimationPlaying);
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<bool> LoadImageAsync(NUIImage image, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken));
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public async Task<bool> LoadImageAsync(NUIImage image, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (image == null)
				return false;
			var path = ResourcePath.GetPath(imageSource);
			return await image.LoadAsync(path);
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public async Task<bool> LoadImageAsync(NUIImage image, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (image == null)
				return false;

			if (imageSource is UriImageSource src)
			{
				return await image.LoadAsync(src.Uri.AbsoluteUri);
			}
			else
			{
				return false;
			}
		}
	}

	public sealed class StreamImageSourceHandler : IImageSourceHandler
	{
		public async Task<bool> LoadImageAsync(NUIImage image, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (image == null)
				return false;

			if (imageSource is StreamImageSource streamsource && streamsource.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)streamsource).GetStreamAsync(cancellationToken))
				{
					if (streamImage != null && image is NImage nimage)
						return await nimage.LoadAsync(streamImage);
				}
			}
			return false;
		}
	}

	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		public Task<bool> LoadImageAsync(NUIImage image, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotSupportedException($"FontImageSource: {imageSource}");
		}
	}
}
