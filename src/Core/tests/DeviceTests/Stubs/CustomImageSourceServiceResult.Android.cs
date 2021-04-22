using System;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class CustomImageSourceServiceResult : IImageSourceServiceResult<Drawable>
	{
		Action _dispose;

		public CustomImageSourceServiceResult(Drawable drawable, Action dispose)
		{
			Value = drawable;
			_dispose = dispose;
		}

		public Drawable Value { get; }

		public void Dispose()
		{
			_dispose.Invoke();
			_dispose = null;
		}
	}
}