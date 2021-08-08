using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class ImageElementManager
	{
		static bool _nativeAnimationSupport = false;
		static ImageElementManager()
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay"))
				if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying"))
					if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Microsoft.UI.Xaml.Media.Imaging.BitmapImage", "Play"))
						if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Microsoft.UI.Xaml.Media.Imaging.BitmapImage", "Stop"))
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

		static WStretch GetStretch(Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return WStretch.Fill;
				case Aspect.AspectFill:
					return WStretch.UniformToFill;
				default:
				case Aspect.AspectFit:
					return WStretch.Uniform;
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
			}
			finally
			{
				imageController?.SetIsLoading(false);
			}
		}

		static internal void RefreshImage(IImageVisualElementRenderer renderer)
		{
			if(renderer.Element is IViewController element)
				element?.InvalidateMeasure(InvalidationTrigger.RendererReady);

			if(renderer.Element is IImageElement controller)
				StartStopAnimation(renderer, controller);
		}
	}
}
