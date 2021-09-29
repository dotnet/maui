#nullable enable
using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIImage;
#elif MONOANDROID
using NativeView = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
	public class ImageSourceServiceResult : IImageSourceServiceResult<NativeView>
	{
		Action? _dispose;

		public ImageSourceServiceResult(NativeView image, Action? dispose = null)
			: this(image, false, dispose)
		{
		}

		public ImageSourceServiceResult(NativeView image, bool resolutionDependent, Action? dispose = null)
		{
			Value = image;
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

		public NativeView Value { get; }

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