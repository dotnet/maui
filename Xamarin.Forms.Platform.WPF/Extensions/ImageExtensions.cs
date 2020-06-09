using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using WImageSource = System.Windows.Media.ImageSource;
using WStretch = System.Windows.Media.Stretch;

namespace Xamarin.Forms.Platform.WPF
{
	public static class ImageExtensions
	{
		public static WStretch ToStretch(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return WStretch.Fill;
				case Aspect.AspectFill:
					return WStretch.UniformToFill;
				default:
				case Aspect.AspectFit:
					return WStretch.Uniform;
			}
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
