using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.Internals;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ImageRenderer : ViewRenderer<Image, Windows.UI.Xaml.Controls.Image>
	{
		bool _measured;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Control.Source == null)
				return new SizeRequest();

			_measured = true;

			var result = new Size { Width = ((BitmapSource)Control.Source).PixelWidth, Height = ((BitmapSource)Control.Source).PixelHeight };

			return new SizeRequest(result);
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
			{
				Control.ImageOpened -= OnImageOpened;
				Control.ImageFailed -= OnImageFailed;
			}

			base.Dispose(disposing);
		}

		protected override async void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var image = new Windows.UI.Xaml.Controls.Image();
					image.ImageOpened += OnImageOpened;
					image.ImageFailed += OnImageFailed;
					SetNativeControl(image);
				}

				await UpdateSource();
				UpdateAspect();
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				await UpdateSource();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect();
		}

		static Stretch GetStretch(Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return Stretch.Fill;
				case Aspect.AspectFill:
					return Stretch.UniformToFill;
				default:
				case Aspect.AspectFit:
					return Stretch.Uniform;
			}
		}

		void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
		{
			if (_measured)
			{
				RefreshImage();
			}

			Element?.SetIsLoading(false);
		}

		void OnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
		{
			Log.Warning("Image Loading", $"Image failed to load: {exceptionRoutedEventArgs.ErrorMessage}" );
			Element?.SetIsLoading(false);
		}

		void RefreshImage()
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.RendererReady);
		}

		void UpdateAspect()
		{
			Control.Stretch = GetStretch(Element.Aspect);
			if (Element.Aspect == Aspect.AspectFill) // Then Center Crop
			{
				Control.HorizontalAlignment = HorizontalAlignment.Center;
				Control.VerticalAlignment = VerticalAlignment.Center;
			}
			else // Default
			{
				Control.HorizontalAlignment = HorizontalAlignment.Left;
				Control.VerticalAlignment = VerticalAlignment.Top;
			}
		}

		async Task UpdateSource()
		{
			Element.SetIsLoading(true);

			ImageSource source = Element.Source;
			IImageSourceHandler handler;
			if (source != null && (handler = Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				Windows.UI.Xaml.Media.ImageSource imagesource = null;

				try
				{
					imagesource = await handler.LoadImageAsync(source);
				}
				catch (OperationCanceledException)
				{
					imagesource = null;
				}
				catch (Exception ex)
				{
					Log.Warning("Image Loading", $"Error updating image source: {ex}");
				}

				// In the time it takes to await the imagesource, some zippy little app
				// might have disposed of this Image already.
				if (Control != null)
				{
					Control.Source = imagesource;
				}

				RefreshImage();
			}
			else
			{
				Control.Source = null;
				Element?.SetIsLoading(false);
			}
		}
	}
}
