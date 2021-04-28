using System;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class CustomImageSourceServiceResult : IImageSourceServiceResult<UIImage>
	{
		Action _dispose;

		public CustomImageSourceServiceResult(UIImage image, Action dispose)
			: this(image, false, dispose)
		{
		}

		public CustomImageSourceServiceResult(UIImage image, bool resolutionDependent, Action dispose)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public UIImage Value { get; }

		public bool IsResolutionDependent { get; }

		public void Dispose()
		{
			_dispose.Invoke();
			_dispose = null;
		}
	}
}