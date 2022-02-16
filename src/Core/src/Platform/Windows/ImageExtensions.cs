using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Microsoft.Maui.Platform
{
	public static class ImageExtensions
	{
		public static Size GetImageSourceSize(this ImageSource source, FrameworkElement? element = null)
		{
			if (source is null)
			{
				return Size.Zero;
			}
			else if (source is BitmapSource bitmap)
			{
				var rasterizationScale = element?.XamlRoot?.RasterizationScale ?? 1;

				return new Size
				{
					Width = bitmap.PixelWidth / rasterizationScale,
					Height = bitmap.PixelHeight / rasterizationScale
				};
			}
			else if (source is CanvasImageSource canvas)
			{
				return new Size
				{
					Width = canvas.Size.Width,
					Height = canvas.Size.Height
				};
			}

			throw new InvalidCastException($"\"{source.GetType().FullName}\" is not supported.");
		}

		internal static string GetFileLocation(this IImageSourceServiceConfiguration? configuration, string filename)
		{
			var imageDirectory = configuration?.GetImageDirectory();
			if (!string.IsNullOrEmpty(imageDirectory))
			{
				var directory = Path.GetDirectoryName(filename);
				if (string.IsNullOrEmpty(directory) || !Path.GetFullPath(directory).Equals(Path.GetFullPath(imageDirectory)))
					filename = Path.Combine(imageDirectory, filename);
			}

			return filename;
		}

		public static IconSource? ToIconSource(this IImageSource source, IMauiContext mauiContext)
		{
			IconSource? image = null;

			if (source is IFileImageSource fis)
			{
				var configuration = mauiContext.Services?.GetService<IImageSourceServiceConfiguration>();
				string filename = configuration.GetFileLocation(fis.File);
				image = new BitmapIconSource { UriSource = new Uri("ms-appx:///" + filename) };
			}
			else if (source is IUriImageSource uri)
			{
				image = new BitmapIconSource { UriSource = uri?.Uri };
			}
			else if (source is IFontImageSource fontImageSource)
			{
				image = new FontIconSource
				{
					Glyph = fontImageSource.Glyph,
					FontSize = fontImageSource.Font.Size
				};

				if (fontImageSource.Color != null)
					image.Foreground = fontImageSource.Color.ToPlatform();

				var fontManager = mauiContext.Services.GetRequiredService<IFontManager>();
				var uwpFontFamily = fontManager.GetFontFamily(fontImageSource.Font);

				if (!string.IsNullOrEmpty(uwpFontFamily.Source))
					((FontIconSource)image).FontFamily = uwpFontFamily;
			}

			return image;
		}
	}
}