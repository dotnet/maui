using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Microsoft.Maui
{
	public static class ImageExtensions
	{
		public static Size GetImageSourceSize(this ImageSource source)
		{
			if (source is null)
			{
				return Size.Zero;
			}
			else if (source is BitmapSource bitmap)
			{
				return new Size
				{
					Width = bitmap.PixelWidth,
					Height = bitmap.PixelHeight
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