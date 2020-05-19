using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Media;
using global::Windows.UI.Xaml.Media.Imaging;
using System.Maui.Internals;
using System.Maui.Platform.UWP;

namespace System.Maui.Platform.UWP
{
	public static class ImageElementManager
	{
		static bool _nativeAnimationSupport = false;
		static ImageElementManager()
		{
			if (global::Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("global::Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay"))
				if (global::Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("global::Windows.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying"))
					if (global::Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("global::Windows.UI.Xaml.Media.Imaging.BitmapImage", "Play"))
						if (global::Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("global::Windows.UI.Xaml.Media.Imaging.BitmapImage", "Stop"))
							_nativeAnimationSupport = true;
		}

		public static void Init(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		internal static void Dispose(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			renderer.ControlChanged -= OnControlChanged;
		}

		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IImageVisualElementRenderer renderer = sender as IImageVisualElementRenderer;
			var controller = renderer.Element as IImageElement;

			if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect(renderer, controller);
			else if (e.PropertyName == Image.IsAnimationPlayingProperty.PropertyName)
				StartStopAnimation(renderer, controller);
		}


		static void StartStopAnimation(IImageVisualElementRenderer renderer, IImageElement controller)
		{
			if (renderer.IsDisposed || controller == null)
			{
				return;
			}

			if (controller.IsLoading)
				return;


			if (renderer.GetImage()?.Source is BitmapImage bitmapImage)
			{
				if (_nativeAnimationSupport)
				{
					if (controller.IsAnimationPlaying && !bitmapImage.IsPlaying)
						bitmapImage.Play();
					else if (!controller.IsAnimationPlaying && bitmapImage.IsPlaying)
						bitmapImage.Stop();

					bitmapImage.RegisterPropertyChangedCallback(BitmapImage.IsPlayingProperty, OnIsPlaying);
				}
			}
		}

		static void OnIsPlaying(DependencyObject sender, DependencyProperty dp)
		{
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				IImageVisualElementRenderer renderer = sender as IImageVisualElementRenderer;
				var controller = renderer.Element as IImageElement;

				UpdateAspect(renderer, controller);
			}
		}



		static void OnControlChanged(object sender, EventArgs e)
		{
			IImageVisualElementRenderer renderer = sender as IImageVisualElementRenderer;

			var controller = renderer.Element as IImageElement;

			UpdateAspect(renderer, controller);
		}

		public static void UpdateAspect(IImageVisualElementRenderer renderer, IImageElement controller)
		{
			var Element = renderer.Element;
			var Control = renderer.GetNativeElement();
			var image = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			image.Stretch = GetStretch(controller.Aspect);
			if (controller.Aspect == Aspect.AspectFill || controller.Aspect == Aspect.AspectFit)

			{
				image.HorizontalAlignment = HorizontalAlignment.Center;
				image.VerticalAlignment = VerticalAlignment.Center;
			}
			else
			{
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
			}
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

		public static async Task UpdateSource(IImageVisualElementRenderer renderer)
		{
			var Element = renderer.Element;
			var Control = renderer.GetNativeElement();
			var imageElement = Element as IImageElement;

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			ImageSource placeholderError = null;

			if (imageElement is Image img)
			{
				placeholderError = img.ErrorPlaceholder;
				var source = await img.LoadingPlaceholder.ToWindowsImageSourceAsync();
				renderer.SetImage(source);
			}

			var imageController = Element as IImageController;

			imageController?.SetIsLoading(true);
			try
			{
				var imagesource = await imageElement.Source.ToWindowsImageSourceAsync();

				if (renderer.IsDisposed)
					return;


				if (imagesource is BitmapImage bitmapImage && _nativeAnimationSupport)
					bitmapImage.AutoPlay = false;

				if (Control != null)
					renderer.SetImage(imagesource);

				RefreshImage(renderer);

				// The ImageFailed event don't trigger when the local ImageSource is invalid, so we need to check the size.
				var size = renderer.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				if (size.Request.IsZero && imageElement is Image)
				{
					imagesource = await placeholderError.ToWindowsImageSourceAsync();
					renderer.SetImage(imagesource);
				}
			}
			finally
			{
				imageController?.SetIsLoading(false);
			}
		}

		static internal void RefreshImage(IImageVisualElementRenderer renderer)
		{
			if (renderer.Element is IViewController element)
				element?.InvalidateMeasure(InvalidationTrigger.RendererReady);

			if (renderer.Element is IImageElement controller)
				StartStopAnimation(renderer, controller);
		}
	}
}
