#nullable enable
using System;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<bool>
	{
		Action? _dispose;

		public ImageSourceServiceResult(bool result, Action? dispose = null)
			: this(result, false, dispose)
		{
		}

		public ImageSourceServiceResult(bool result, bool resolutionDependent, Action? dispose = null)
		{
			Value = result;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public bool Value { get; }

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