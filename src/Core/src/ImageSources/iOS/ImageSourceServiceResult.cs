#nullable enable
using System;
using UIKit;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<UIImage>
	{
		Action? _dispose;

		public ImageSourceServiceResult(UIImage image, Action? dispose = null)
			: this(image, false, dispose)
		{
		}

		public ImageSourceServiceResult(UIImage image, bool resolutionDependent, Action? dispose = null)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public UIImage Value { get; }

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