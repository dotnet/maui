using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.Forms.Internals;
using WBrush = System.Windows.Media.Brush;
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

			var source = Element.Source;

			Element.SetIsLoading(true);
			try
			{
				var imagesource = await source.ToWindowsImageSourceAsync();

				// only set if we are still on the same image
				if (Control != null && Element.Source == source)
					Control.Source = imagesource;
			}
			finally
			{
				// only mark as finished if we are still on the same image
				if (Element.Source == source)
					Element.SetIsLoading(false);
			}

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
			BitmapImage bitmapImage = null;
			StreamImageSource streamImageSource = imagesource as StreamImageSource;

			if (streamImageSource != null && streamImageSource.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamImageSource).GetStreamAsync(cancelationToken))
				{
					bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.StreamSource = stream;
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.EndInit();
				}
			}

			return bitmapImage;
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancelationToken = new CancellationToken())
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
			return Task.FromResult<System.Windows.Media.ImageSource>(bitmapimage);
		}
	}

	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		public Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = new CancellationToken())
		{
			var fontsource = imagesource as FontImageSource;
			var image = CreateGlyph(
					fontsource.Glyph,
					new FontFamily(new Uri("pack://application:,,,"), fontsource.FontFamily),
					FontStyles.Normal,
					FontWeights.Normal,
					FontStretches.Normal,
					fontsource.Size,
					(fontsource.Color != Color.Default ? fontsource.Color : Color.White).ToBrush());
			return Task.FromResult(image);
		}

		static System.Windows.Media.ImageSource CreateGlyph(
			string text,
			FontFamily fontFamily,
			FontStyle fontStyle,
			FontWeight fontWeight,
			FontStretch fontStretch,
			double fontSize,
			WBrush foreBrush)
		{
			if (fontFamily == null || string.IsNullOrEmpty(text))
			{
				return null;
			}
			var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
			if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
			{
				//if it does not work 
				return null;
			}

			var glyphIndexes = new ushort[text.Length];
			var advanceWidths = new double[text.Length];
			for (int n = 0; n < text.Length; n++)
			{
				var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
				glyphIndexes[n] = glyphIndex;
				var width = glyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
				advanceWidths[n] = width;
			}

#if NETCOREAPP3_1
			var dpi = VisualTreeHelper.GetDpi(System.Windows.Application.Current.MainWindow).PixelsPerDip;
			var gr = new GlyphRun(glyphTypeface,
				0,
				false,
				fontSize,
				(float)dpi,
				glyphIndexes,
				new System.Windows.Point(0, 0),
				advanceWidths,
				null, null, null, null, null, null);
#else
			var gr = new GlyphRun(glyphTypeface,
				0, false,
				fontSize,
				glyphIndexes,
				new System.Windows.Point(0, 0),
				advanceWidths,
				null, null, null, null, null, null);
#endif
			var glyphRunDrawing = new GlyphRunDrawing(foreBrush, gr);
			return new DrawingImage(glyphRunDrawing);
		}
	}
}