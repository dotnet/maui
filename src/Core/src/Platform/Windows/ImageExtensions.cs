using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

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
	}
}