using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public static class ImageExtensions
	{
		public static UIViewContentMode ToUIViewContentMode(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.AspectFill:
					return UIViewContentMode.ScaleAspectFill;
				case Aspect.Fill:
					return UIViewContentMode.ScaleToFill;
				case Aspect.AspectFit:
				default:
					return UIViewContentMode.ScaleAspectFit;
			}
		}
	}

	public class ImageRenderer : ViewRenderer<Image, UIImageView>
	{
		bool _isDisposed;

		IElementController ElementController => Element as IElementController;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				UIImage oldUIImage;
				if (Control != null && (oldUIImage = Control.Image) != null)
				{
					oldUIImage.Dispose();
					oldUIImage = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				var imageView = new UIImageView(RectangleF.Empty);
				imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
				imageView.ClipsToBounds = true;
				SetNativeControl(imageView);
			}

			if (e.NewElement != null)
			{
				SetAspect();
				SetImage(e.OldElement);
				SetOpacity();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Image.SourceProperty.PropertyName)
				SetImage();
			else if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
				SetOpacity();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect();
		}

		void SetAspect()
		{
			Control.ContentMode = Element.Aspect.ToUIViewContentMode();
		}

		async void SetImage(Image oldElement = null)
		{
			var source = Element.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource && source is FileImageSource && ((FileImageSource)oldSource).File == ((FileImageSource)source).File)
					return;

				Control.Image = null;
			}

			IImageSourceHandler handler;

			Element.SetIsLoading(true);

			if (source != null && (handler = Internals.Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				UIImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source, scale: (float)UIScreen.MainScreen.Scale);
				}
				catch (OperationCanceledException)
				{
					uiimage = null;
				}

				var imageView = Control;
				if (imageView != null)
					imageView.Image = uiimage;

				if (!_isDisposed)
					((IVisualElementController)Element).NativeSizeChanged();
			}
			else
				Control.Image = null;

			if (!_isDisposed)
				Element.SetIsLoading(false);
		}

		void SetOpacity()
		{
			Control.Opaque = Element.IsOpaque;
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1);
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage image = null;
			var filesource = imagesource as FileImageSource;
			if (filesource != null)
			{
				var file = filesource.File;
				if (!string.IsNullOrEmpty(file))
					image = File.Exists(file) ? new UIImage(file) : UIImage.FromBundle(file);
			}
			return Task.FromResult(image);
		}
	}

	public sealed class StreamImagesourceHandler : IImageSourceHandler
	{
		public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage image = null;
			var streamsource = imagesource as StreamImageSource;
			if (streamsource != null && streamsource.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
						image = UIImage.LoadFromData(NSData.FromStream(streamImage), scale);
				}
			}
			return image;
		}
	}

	public sealed class ImageLoaderSourceHandler : IImageSourceHandler
	{
		public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage image = null;
			var imageLoader = imagesource as UriImageSource;
			if (imageLoader != null && imageLoader.Uri != null)
			{
				using (var streamImage = await imageLoader.GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
						image = UIImage.LoadFromData(NSData.FromStream(streamImage), scale);
				}
			}
			return image;
		}
	}
}