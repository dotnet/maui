using System;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<WImageSource>
	{
		Action? _dispose;

		public ImageSourceServiceResult(WImageSource image, Action? dispose = null)
		{
			Value = image;
			_dispose = dispose;
		}

		public WImageSource Value { get; }

		public void Dispose()
		{
			_dispose?.Invoke();
			_dispose = null;
		}
	}
}