using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public static class ImageElementManager
	{
		public static void Init(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		public static void Dispose(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			renderer.ControlChanged -= OnControlChanged;
		}


		static void OnControlChanged(object sender, EventArgs e)
		{
			var renderer = sender as IImageVisualElementRenderer;
			var imageElement = renderer.Element as IImageElement;
			SetAspect(renderer, imageElement);
			SetOpacity(renderer, imageElement);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				var renderer = sender as IImageVisualElementRenderer;
				var imageElement = renderer.Element as IImageElement;

				SetAspect(renderer, imageElement);
				SetOpacity(renderer, imageElement);
			}
		}

		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var renderer = sender as IImageVisualElementRenderer;
			var imageElement = renderer.Element as IImageElement;

			if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
				SetOpacity(renderer, renderer.Element as IImageElement);
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect(renderer, renderer.Element as IImageElement);
		}



		public static void SetAspect(IImageVisualElementRenderer renderer, IImageElement imageElement)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();


			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			Control.ContentMode = imageElement.Aspect.ToUIViewContentMode();
		}

		public static void SetOpacity(IImageVisualElementRenderer renderer, IImageElement imageElement)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			Control.Opaque = imageElement.IsOpaque;
		}

		public static async Task SetImage(IImageVisualElementRenderer renderer, IImageElement imageElement, Image oldElement = null)
		{
			_ = renderer ?? throw new ArgumentNullException($"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(renderer)} cannot be null");
			_ = imageElement ?? throw new ArgumentNullException($"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(imageElement)} cannot be null");

			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			var imageController = imageElement as IImageController;

			var source = imageElement.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource && source is FileImageSource && ((FileImageSource)oldSource).File == ((FileImageSource)source).File)
					return;

				renderer.SetImage(null);
			}

			IImageSourceHandler handler;
			imageController?.SetIsLoading(true);
			try
			{
				if (source != null &&
				   (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
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

					if (renderer.IsDisposed)
						return;

					var imageView = Control;
					if (imageView != null)
					{
						renderer.SetImage(uiimage);
					}
				}
				else
				{
					renderer.SetImage(null);
				}

			}
			finally
			{
				imageController?.SetIsLoading(false);
			}

			(imageElement as IViewController)?.NativeSizeChanged();
		}
	}
}