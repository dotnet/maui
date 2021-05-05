#nullable enable
using System;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<WImageSource>
	{
		Action? _dispose;

		public ImageSourceServiceResult(WImageSource image, Action? dispose = null)
			: this(image, false, dispose)
		{
		}

		public ImageSourceServiceResult(WImageSource image, bool resolutionDependent, Action? dispose = null)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public WImageSource Value { get; }

		public bool IsResolutionDependent { get; }

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			if (IsDisposed)
				return;

			IsDisposed = true;

			_dispose?.Invoke();
			_dispose = null;
		}
	}
}