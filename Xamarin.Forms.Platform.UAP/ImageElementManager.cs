using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.Platform.UWP
{
	public static class ImageElementManager
	{
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
			var controller = renderer.Element as IImageController;

			if (e.PropertyName == controller.AspectProperty?.PropertyName)
				UpdateAspect(renderer, controller);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				IImageVisualElementRenderer renderer = sender as IImageVisualElementRenderer;
				var controller = renderer.Element as IImageController;

				UpdateAspect(renderer, controller);
			}
		}



		static void OnControlChanged(object sender, EventArgs e)
		{
			IImageVisualElementRenderer renderer = sender as IImageVisualElementRenderer;

			var controller = renderer.Element as IImageController;

			UpdateAspect(renderer, controller);
		}

		public static void UpdateAspect(IImageVisualElementRenderer renderer, IImageController controller)
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
			var controller = Element as IImageController;

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			controller.SetIsLoading(true);

			ImageSource source = controller.Source;
			IImageSourceHandler handler;
			if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				Windows.UI.Xaml.Media.ImageSource imagesource;

				try
				{
					imagesource = await handler.LoadImageAsync(source);
				}
				catch (OperationCanceledException)
				{
					imagesource = null;
				}

				// In the time it takes to await the imagesource, some zippy little app
				// might have disposed of this Image already.
				if (Control != null)
				{
					renderer.SetImage(imagesource);
				}

				RefreshImage(controller as IViewController);
			}
			else
			{
				renderer.SetImage(null);
				controller.SetIsLoading(false);
			}
		}


		static internal void RefreshImage(IViewController element)
		{
			element?.InvalidateMeasure(InvalidationTrigger.RendererReady);
		}


	}
}
