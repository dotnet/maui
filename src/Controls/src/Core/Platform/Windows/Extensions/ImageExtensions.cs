using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ImageExtensions
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

		public static Microsoft.UI.Xaml.Controls.IconSource ToWindowsIconSource(this ImageSource source)
		{
			return source.ToWindowsIconSourceAsync().GetAwaiter().GetResult();
		}

		public static Task<Microsoft.UI.Xaml.Controls.IconSource> ToWindowsIconSourceAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Task.FromResult<IconSource>(null);
			// TODO MAUI

			//if (source == null || source.IsEmpty)
			//	Task.FromResult<WImageSource>(null);

			//var handler = Registrar.Registered.GetHandlerForObject<IIconElementHandler>(source);
			//if (handler == null)
			//	Task.FromResult<WImageSource>(null);

			//try
			//{
			//	return await handler.LoadIconSourceAsync(source, cancellationToken);
			//}
			//catch (OperationCanceledException)
			//{
			//	// no-op
			//}

			//Task.FromResult<WImageSource>(null);
		}

		public static IconElement ToWindowsIconElement(this ImageSource source)
		{
			return source.ToWindowsIconElementAsync().GetAwaiter().GetResult();
		}

		public static Task<IconElement> ToWindowsIconElementAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			//TODO MAUI
			return Task.FromResult<IconElement>(null);

			//if (source == null || source.IsEmpty)
			//	Task.FromResult<WImageSource>(null);

			//var handler = Registrar.Registered.GetHandlerForObject<IIconElementHandler>(source);
			//if (handler == null)
			//	Task.FromResult<WImageSource>(null);

			//try
			//{
			//	return await handler.LoadIconElementAsync(source, cancellationToken);
			//}
			//catch (OperationCanceledException)
			//{
			//	// no-op
			//}

			//Task.FromResult<WImageSource>(null);
		}

		public static WImageSource ToWindowsImageSource(this ImageSource source)
		{
			return source.ToWindowsImageSourceAsync().GetAwaiter().GetResult();
		}

		public static Task<WImageSource> ToWindowsImageSourceAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			// TODO MAUI
			return Task.FromResult<WImageSource>(null);

//			if (source == null || source.IsEmpty)
//				Task.FromResult<WImageSource>(null);

//			var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source);
//			if (handler == null)
//				Task.FromResult<WImageSource>(null);

//			try
//			{
//				return await handler.LoadImageAsync(source, cancellationToken);
//			}
//			catch (OperationCanceledException)
//			{
//				Log.Warning("Image loading", "Image load cancelled");
//			}
//			catch (Exception ex)
//			{
//				Log.Warning("Image loading", $"Image load failed: {ex}");
//#if DEBUG
//				throw;
//#endif
//			}

//			Task.FromResult<WImageSource>(null);
		}
	}
}
