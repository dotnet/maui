#nullable enable
using System;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIImage;
#elif MONOANDROID
using PlatformView = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<PlatformView>
	{
		Action? _dispose;

		public ImageSourceServiceResult(PlatformView image, Action? dispose = null)
			: this(image, false, dispose)
		{
		}

		public ImageSourceServiceResult(PlatformView image, bool resolutionDependent, Action? dispose = null)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public PlatformView Value { get; }

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