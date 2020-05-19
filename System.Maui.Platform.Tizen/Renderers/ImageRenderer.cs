using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using EImage = ElmSharp.Image;

using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Image;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ImageRenderer : ViewRenderer<Image, Native.Image>
	{
		public ImageRenderer()
		{
			RegisterPropertyHandler(Image.SourceProperty, UpdateSource);
			RegisterPropertyHandler(Image.AspectProperty, UpdateAspect);
			RegisterPropertyHandler(Image.IsOpaqueProperty, UpdateIsOpaque);
			RegisterPropertyHandler(Image.IsAnimationPlayingProperty, UpdateIsAnimationPlaying);
			RegisterPropertyHandler(Specific.BlendColorProperty, UpdateBlendColor);
			RegisterPropertyHandler(Specific.FileProperty, UpdateFile);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.Image(Forms.NativeParent));
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
					((IVisualElementController)Element).NativeSizeChanged();
					UpdateAfterLoading(initialize);
				}
			}

			if (!IsDisposed)
				((IImageController)Element).SetIsLoading(false);
		}

		void UpdateFile(bool initialize)
		{
			if (initialize && Specific.GetFile(Element) == default || Element.Source != default(ImageSource))
				return;

			if (Control != null)
			{
				bool success = Control.LoadFromFile(Specific.GetFile(Element));

				if (!IsDisposed && success)
				{
					((IVisualElementController)Element).NativeSizeChanged();
					UpdateAfterLoading(initialize);
				}
			}
		}

		protected virtual void UpdateAfterLoading(bool initialize)
		{
			UpdateIsOpaque(initialize);
			UpdateBlendColor(initialize);
			UpdateIsAnimationPlaying(initialize);
		}

		void UpdateAspect(bool initialize)
		{
			if (initialize && Element.Aspect == Aspect.AspectFit)
				return;

			Control.ApplyAspect(Element.Aspect);
		}

		void UpdateIsOpaque(bool initialize)
		{
			if (initialize && !Element.IsOpaque)
				return;

			Control.IsOpaque = Element.IsOpaque;
		}

		void UpdateIsAnimationPlaying(bool initialize)
		{
			if (initialize && !Element.IsAnimationPlaying)
				return;

			Control.IsAnimated = Element.IsAnimationPlaying;
			Control.IsAnimationPlaying = Element.IsAnimationPlaying;
		}

		void UpdateBlendColor(bool initialize)
		{
			if (initialize && Specific.GetBlendColor(Element).IsDefault)
				return;

			Control.Color = Specific.GetBlendColor(Element).ToNative();
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<bool> LoadImageAsync(EImage image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken));
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<bool> LoadImageAsync(EImage image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			var filesource = imageSource as FileImageSource;
			if (filesource != null)
			{
				string file = filesource.File;
				if (!string.IsNullOrEmpty(file))
					return image.LoadAsync(ResourcePath.GetPath(file), cancelationToken);
			}
			return Task.FromResult<bool>(false);
		}
	}

	public sealed class StreamImageSourceHandler : IImageSourceHandler
	{
		public async Task<bool> LoadImageAsync(EImage image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			var streamsource = imageSource as StreamImageSource;
			if (streamsource != null && streamsource.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken))
				{
					if (streamImage != null)
						return await image.LoadAsync(streamImage, cancelationToken);
				}
			}
			return false;
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public Task<bool> LoadImageAsync(EImage image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			var urisource = imageSource as UriImageSource;
			if (urisource != null && urisource.Uri != null)
			{
				return image.LoadAsync(urisource.Uri, cancelationToken);
			}

			return Task.FromResult<bool>(false);
		}
	}
}
