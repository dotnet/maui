using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal static class ImageExtensions
	{
		public static Stretch ToStretch(this Aspect aspect)
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
	}

	public class ImageRenderer : ViewRenderer<Image, System.Windows.Controls.Image>
	{
		IElementController ElementController => Element as IElementController;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			// Someone reported a NRE happening in this method which can only be explained by Control being null
			// which only happens at the very beginning of the view lifecycle. Honest I have no idea how this might 
			// happen because it really shouldn't measure at that point. Add check anyway and live in fear...
			if (Control?.Source == null)
				return new SizeRequest();

			var result = new Size { Width = ((BitmapImage)Control.Source).PixelWidth, Height = ((BitmapImage)Control.Source).PixelHeight };

			return new SizeRequest(result);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var image = new System.Windows.Controls.Image();
					SetNativeControl(image);
				}

				SetSource(Control);
				SetAspect(Control);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				SetSource(Control);
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect(Control);
		}

		void SetAspect(System.Windows.Controls.Image image)
		{
			Aspect aspect = Element.Aspect;

			image.Stretch = aspect.ToStretch();
		}

		async void SetSource(System.Windows.Controls.Image image)
		{
			((IImageController)Element).SetIsLoading(true);

			ImageSource source = Element.Source;
			IImageSourceHandler handler;
			if (source != null && (handler = Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				System.Windows.Media.ImageSource imagesource;
				try
				{
					imagesource = await handler.LoadImageAsync(source);
				}
				catch (TaskCanceledException)
				{
					imagesource = null;
				}
				image.Source = imagesource;
				// if you dont at least measure the thing once it wont load the image
				// then the whole thing falls over.
				image.Measure(new System.Windows.Size(100, 100));
				((IVisualElementController)Element).NativeSizeChanged();
			}
			else
				image.Source = null;

			((IImageController)Element).SetIsLoading(false);
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = default(CancellationToken));
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = new CancellationToken())
		{
			System.Windows.Media.ImageSource image = null;
			var filesource = imagesoure as FileImageSource;
			if (filesource != null)
			{
				string file = filesource.File;
				image = new BitmapImage(new Uri("/" + file, UriKind.Relative));
			}
			return Task.FromResult(image);
		}
	} 

	public sealed class StreamImagesourceHandler : IImageSourceHandler
	{
		public async Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = new CancellationToken())
		{
			BitmapImage bitmapimage = null;

			var streamsource = imagesource as StreamImageSource;
			if (streamsource != null && streamsource.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken))
				{
					bitmapimage = new BitmapImage();
					bitmapimage.SetSource(stream);
				}
			}
			return (System.Windows.Media.ImageSource)bitmapimage;
		}
	}

	public sealed class ImageLoaderSourceHandler : IImageSourceHandler
	{
		public async Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = new CancellationToken())
		{
			BitmapImage bitmapimage = null;
			var imageLoader = imagesoure as UriImageSource;
			if (imageLoader != null && imageLoader.Uri != null)
			{
				using (Stream streamimage = await imageLoader.GetStreamAsync(cancelationToken))
				{
					if (streamimage != null && streamimage.CanRead)
					{
						bitmapimage = new BitmapImage();
						bitmapimage.SetSource(streamimage);
					}
				}
			}
			return bitmapimage;
		}
	}
}