using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using global::Windows.UI.Xaml.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using global::Windows.Foundation;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Input;
using global::Windows.UI.Xaml.Media;
using System.Maui.Internals;
using WImageSource = global::Windows.UI.Xaml.Media.ImageSource;

namespace System.Maui.Platform.UWP
{
	internal static class ImageExtensions
	{
		public static Size GetImageSourceSize(this WImageSource source)
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

		public static Microsoft.UI.Xaml.Controls.IconSource ToWindowsIconSource(this ImageSource source)
		{
			return source.ToWindowsIconSourceAsync().GetAwaiter().GetResult();
		}

		public static async Task<Microsoft.UI.Xaml.Controls.IconSource> ToWindowsIconSourceAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null || source.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IIconElementHandler>(source);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadIconSourceAsync(source, cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// no-op
			}

			return null;
		}

		public static IconElement ToWindowsIconElement(this ImageSource source)
		{
			return source.ToWindowsIconElementAsync().GetAwaiter().GetResult();
		}

		public static async Task<IconElement> ToWindowsIconElementAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null || source.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IIconElementHandler>(source);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadIconElementAsync(source, cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// no-op
			}

			return null;
		}

		public static WImageSource ToWindowsImageSource(this ImageSource source)
		{
			return source.ToWindowsImageSourceAsync().GetAwaiter().GetResult();
		}

		public static async Task<WImageSource> ToWindowsImageSourceAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null || source.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadImageAsync(source, cancellationToken);
			}
			catch (OperationCanceledException)
			{
				Log.Warning("Image loading", "Image load cancelled");
			}
			catch (Exception ex)
			{
				Log.Warning("Image loading", $"Image load failed: {ex}");
			}

			return null;
		}
	}
}
