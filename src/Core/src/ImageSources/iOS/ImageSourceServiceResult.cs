using System;
using UIKit;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<UIImage>
	{
		Action? _dispose;

		public ImageSourceServiceResult(UIImage image, Action? dispose = null)
		{
			Value = image;
			_dispose = dispose;
		}

		public UIImage Value { get; }

		public void Dispose()
		{
			_dispose?.Invoke();
			_dispose = null;
		}
	}
}