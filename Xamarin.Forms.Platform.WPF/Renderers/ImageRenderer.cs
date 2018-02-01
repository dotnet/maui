using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.Forms.Internals;
using WImage = System.Windows.Controls.Image;

namespace Xamarin.Forms.Platform.WPF
{
	public class ImageRenderer : ViewRenderer<Image, WImage>
	{
		protected override async void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WImage());
				}

				// Update control property 
				await TryUpdateSource();
				UpdateAspect();
			}

			base.OnElementChanged(e);
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				await TryUpdateSource();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect();
		}
		
		void UpdateAspect()
		{
			Control.Stretch = Element.Aspect.ToStretch();
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

		protected virtual async Task TryUpdateSource()
		{
			try
			{
				await UpdateSource().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
			}
			finally
			{
				Element.SetIsLoading(false);
			}
		}

		protected async Task UpdateSource()
		{
			if (Element == null || Control == null)
			{
				return;
			}

			Element.SetIsLoading(true);
			
			ImageSource source = Element.Source;
			IImageSourceHandler handler;
			if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				System.Windows.Media.ImageSource imagesource;

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
					Control.Source = imagesource;
				}

				RefreshImage();
			}
			else
			{
				Control.Source = null;
				Element.SetIsLoading(false);
			}
		}

		void RefreshImage()
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.RendererReady);
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
			FileImageSource filesource = imagesoure as FileImageSource;
			if (filesource != null)
			{
				string file = filesource.File;
				image = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
			}
			return Task.FromResult(image);
		}
	}

	public sealed class StreamImageSourceHandler : IImageSourceHandler
	{
		public async Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = new CancellationToken())
		{
			BitmapImage bitmapimage = null;
			StreamImageSource streamsource = imagesource as StreamImageSource;
			if (streamsource != null && streamsource.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken))
				{
					bitmapimage = new BitmapImage()
					{
						StreamSource = stream
					};
				}
			}
			return bitmapimage;
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public async Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = new CancellationToken())
		{
			BitmapImage bitmapimage = null;
			var imageLoader = imagesoure as UriImageSource;
			if (imageLoader?.Uri != null)
			{
				bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.UriSource = imageLoader.Uri;
				bitmapimage.EndInit();
			}
			return bitmapimage;
		}
	}
}