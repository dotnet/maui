using System;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class CustomImageSourceServiceResult : IImageSourceServiceResult<Drawable>
	{
		Action _dispose;

		public CustomImageSourceServiceResult(Drawable drawable, Action dispose)
			: this(drawable, false, dispose)
		{
		}

		public CustomImageSourceServiceResult(Drawable drawable, bool resolutionDependent, Action dispose)
		{
			Value = drawable;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public Drawable Value { get; }

		public bool IsResolutionDependent { get; }

		public void Dispose()
		{
			_dispose.Invoke();
			_dispose = null;
		}
	}
}