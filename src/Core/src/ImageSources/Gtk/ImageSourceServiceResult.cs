#nullable enable
using System;
using NativeImage = Gdk.Pixbuf;

namespace Microsoft.Maui
{

	public class ImageSourceServiceResult : IImageSourceServiceResult<NativeImage>
	{

		Action? _dispose;

		public ImageSourceServiceResult(NativeImage image, Action? dispose = null)
			: this(image, false, dispose)
		{ }

		public ImageSourceServiceResult(NativeImage image, bool resolutionDependent, Action? dispose = null)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public NativeImage Value { get; }

		public bool IsResolutionDependent { get; }

		public bool IsDisposed { get; private set; }

		public bool IsResource { get; set; }

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