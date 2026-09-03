using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class ImageExtensions
	{
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
				Application.Current?.FindMauiContext()?.CreateLogger<ImageSource>()?.LogWarning("Image load cancelled");
			}
			catch (Exception ex)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<ImageSource>()?.LogWarning(ex, "Image load failed");
#if DEBUG
				throw;
#endif
			}

			return null;
		}
	}
}
