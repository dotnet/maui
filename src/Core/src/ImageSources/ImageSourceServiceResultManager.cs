#nullable enable
using System;
using System.Threading;

#if IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformImage = Microsoft.Maui.Platform.MauiImageSource;
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformImage = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	internal class ImageSourceServiceResultManager
	{
		CancellationTokenSource? _sourceCancellation;
		IDisposable? _sourceResult;

		public bool IsLoading => _sourceCancellation is not null;
		public CancellationToken Token =>
			_sourceCancellation?.Token ?? default;

		public bool IsResolutionDependent { get; private set; }

		public float CurrentResolution { get; private set; } = 1.0f;

		public CancellationToken BeginLoad()
		{
			_sourceResult?.Dispose();

			_sourceCancellation?.Cancel();
			_sourceCancellation = new CancellationTokenSource();

			return Token;
		}

		public void CompleteLoad<T>(IImageSourceServiceResult<T>? result, PlatformView uiContext)
		{
			CompleteLoad((IDisposable?)result);

			IsResolutionDependent = result?.IsResolutionDependent ?? false;

#if IOS || MACCATALYST || WINDOWS
			CurrentResolution = uiContext.GetDisplayDensity();
#endif
		}

		public bool RequiresReload(PlatformView uiContext)
		{
			if (!IsResolutionDependent)
				return false;

#if IOS || MACCATALYST || WINDOWS
			if (uiContext is not null)
			{
				var newResolution = uiContext.GetDisplayDensity();
				return CurrentResolution != newResolution;
			}
#endif

			return false;
		}

		public void CompleteLoad(IDisposable? result)
		{
			_sourceResult = result;
			_sourceCancellation?.Dispose();
			_sourceCancellation = null;

			IsResolutionDependent = false;
			CurrentResolution = 1.0f;
		}

		public void Reset()
		{
			BeginLoad();
			CompleteLoad(null);
		}
	}
}