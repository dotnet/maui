using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ImageRenderer : ViewRenderer<Image, NSImageView>
	{
		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				NSImage oldUIImage;
				if (Control != null && (oldUIImage = Control.Image) != null)
				{
					oldUIImage.Dispose();
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				var imageView = new FormsNSImageView();
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
			//TODO: Implement set Image Aspect
			//Control.ContentMode = Element.Aspect.ToUIViewContentMode();
		}

		async void SetImage(Image oldElement = null)
		{
			var source = Element.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				var imageSource = oldSource as FileImageSource;
				if (imageSource != null && source is FileImageSource && imageSource.File == ((FileImageSource)source).File)
					return;

				Control.Image = null;
			}

			IImageSourceHandler handler;

			Element.SetIsLoading(true);

			if (source != null && (handler = Internals.Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				NSImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source, scale: (float)NSScreen.MainScreen.BackingScaleFactor);
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
			(Control as FormsNSImageView)?.SetIsOpaque(Element.IsOpaque);
		}
	}
}