using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using WinImageSource = Windows.UI.Xaml.Media.ImageSource;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class ImageExtensions
	{
		public static SizeRequest GetDesiredSize(this WinImageSource source)
		{
			if (source is BitmapSource bitmap)
			{
				return new SizeRequest(
					new Size
					{
						Width = bitmap.PixelWidth,
						Height = bitmap.PixelHeight
					});
			}
			else if (source is CanvasImageSource canvas)
			{
				return new SizeRequest(
					new Size
					{
						Width = canvas.SizeInPixels.Width,
						Height = canvas.SizeInPixels.Height
					});
			}
			else
			{
				throw new InvalidCastException($"\"{source.GetType().FullName}\" is not supported.");
			}
		}
	}
}
