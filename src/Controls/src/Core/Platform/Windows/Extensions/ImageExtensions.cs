using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Platform
{
	internal static partial class ImageExtensions
	{
		public static Graphics.Size GetImageSourceSize(this WImageSource source)
		{
			if (source is null)
			{
				return Graphics.Size.Zero;
			}
			else if (source is BitmapSource bitmap)
			{
				return new Graphics.Size
				{
					Width = bitmap.PixelWidth,
					Height = bitmap.PixelHeight
				};
			}
			else if (source is CanvasImageSource canvas)
			{
				return new Graphics.Size
				{
					Width = canvas.Size.Width,
					Height = canvas.Size.Height
				};
			}

			throw new InvalidCastException($"\"{source.GetType().FullName}\" is not supported.");
		}
	}
}
