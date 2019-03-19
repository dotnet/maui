using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

#if __MOBILE__
using UIKit;
using NativeImage = UIKit.UIImage;
namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using CoreAnimation;
using NativeImage = AppKit.NSImage;
namespace Xamarin.Forms.Platform.MacOS
#endif
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
#if __MOBILE__
			Control.ContentMode = imageElement.Aspect.ToUIViewContentMode();
#else
			Control.Layer.ContentsGravity = imageElement.Aspect.ToNSViewContentMode();
#endif
		}

		public static void SetOpacity(IImageVisualElementRenderer renderer, IImageElement imageElement)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}
#if __MOBILE__
			Control.Opaque = imageElement.IsOpaque;
#else
			(Control as FormsNSImageView)?.SetIsOpaque(imageElement.IsOpaque);
#endif
		}

		public static async Task SetImage(IImageVisualElementRenderer renderer, IImageElement imageElement, Image oldElement = null)
		{
			_ = renderer ?? throw new ArgumentNullException(nameof(renderer), $"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(renderer)} cannot be null");
			_ = imageElement ?? throw new ArgumentNullException(nameof(imageElement), $"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(imageElement)} cannot be null");

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

			imageController?.SetIsLoading(true);
			try
			{
				var uiimage = await source.GetNativeImageAsync();
				if (renderer.IsDisposed)
					return;

				renderer.SetImage(Control == null ? null : uiimage);
			}
			finally
			{
				imageController?.SetIsLoading(false);
			}

			(imageElement as IViewController)?.NativeSizeChanged();
		}

		internal static async Task<NativeImage> GetNativeImageAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			IImageSourceHandler handler;
			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				try
				{
#if __MOBILE__
					float scale = (float)UIScreen.MainScreen.Scale;
#else
					float scale = (float)NSScreen.MainScreen.BackingScaleFactor;
#endif
					return await handler.LoadImageAsync(source, scale: scale, cancelationToken: cancellationToken);
				}
				catch (OperationCanceledException ex)
				{
					Internals.Log.Warning(source.GetType().Name, "Error loading image: {0}", ex);
				}
			}

			return null;
		}
	}
}