#nullable enable
using System;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIImage;
#elif ANDROID
using PlatformView = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Media.ImageSource;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Image;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	public class ImageSourceServiceLoadResult : IImageSourceServiceResult
	{
		Action? _dispose;

		public ImageSourceServiceLoadResult()
		{
		}

		public ImageSourceServiceLoadResult(Action? dispose = null)
			: this(false, dispose)
		{
		}

		public ImageSourceServiceLoadResult(bool resolutionDependent, Action? dispose = null)
		{
			IsResolutionDependent = resolutionDependent;
			_dispose = dispose;
		}

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