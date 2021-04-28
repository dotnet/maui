#nullable enable
using System;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<Drawable>
	{
		Action? _dispose;

		public ImageSourceServiceResult(Drawable drawable, Action? dispose = null)
			: this(drawable, false, dispose)
		{
		}

		public ImageSourceServiceResult(Drawable drawable, bool resolutionDependent, Action? dispose = null)
		{
			Value = drawable;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public Drawable Value { get; }

		public bool IsResolutionDependent { get; internal set; }

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