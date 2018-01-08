using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Image;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ImageRenderer : ViewRenderer<Image, Native.Image>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				var image = new Native.Image(Forms.NativeParent);
				SetNativeControl(image);
			}

			UpdateAll();
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Image.SourceProperty.PropertyName)
			{
				UpdateSource();
			}
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
			{
				UpdateAspect();
			}
			else if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
			{
				UpdateIsOpaque();
			}
			else if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable && e.PropertyName == "BlendColor")
			{
				UpdateBlendColor();
			}
		}

		async void UpdateSource()
		{
			ImageSource source = Element.Source;

			((IImageController)Element).SetIsLoading(true);

			if (Control != null)
			{
				bool success = await Control.LoadFromImageSourceAsync(source);
				if (!IsDisposed && success)
				{
					((IVisualElementController)Element).NativeSizeChanged();
					UpdateAfterLoading();
				}
			}

			if (!IsDisposed)
				((IImageController)Element).SetIsLoading(false);
		}

		protected virtual void UpdateAfterLoading()
		{
			UpdateIsOpaque();
			if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable)
			{
				UpdateBlendColor();
			}
		}

		void UpdateAspect()
		{
			Control.Aspect = Element.Aspect;
		}

		void UpdateIsOpaque()
		{
			Control.IsOpaque = Element.IsOpaque;
		}

		void UpdateBlendColor()
		{
			Control.Color = Specific.GetBlendColor(Element).ToNative();
		}

		void UpdateAll()
		{
			UpdateSource();
			UpdateAspect();
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<bool> LoadImageAsync(Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken));
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<bool> LoadImageAsync(Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
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
		public async Task<bool> LoadImageAsync(Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
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
		public Task<bool> LoadImageAsync(Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
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
