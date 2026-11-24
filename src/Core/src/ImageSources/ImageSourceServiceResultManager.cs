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
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformImage = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	internal class ImageSourceServiceResultManager
	{
		readonly object _loadLock = new();
		CancellationTokenSource? _sourceCancellation;
		IDisposable? _sourceResult;

		public bool IsLoading
		{
			get
			{
				lock (_loadLock)
				{
					return _sourceCancellation is not null;
				}
			}
		}

		public bool IsResolutionDependent { get; private set; }

		public float CurrentResolution { get; private set; } = 1.0f;

		public CancellationToken BeginLoad()
		{
			var cancellation = new CancellationTokenSource();
			var token = cancellation.Token;
			CancellationTokenSource? previousCancellation;
			IDisposable? previousResult;

			lock (_loadLock)
			{
				previousCancellation = _sourceCancellation;
				previousResult = _sourceResult;
				_sourceCancellation = cancellation;
				_sourceResult = null;
			}

			// Cancellation callbacks can complete the previous load synchronously.
			using (previousCancellation)
			{
				previousCancellation?.Cancel();
			}

			previousResult?.Dispose();
			return token;
		}

		public void CompleteLoad<T>(IImageSourceServiceResult<T>? result, PlatformView uiContext, CancellationToken token)
		{
#if IOS || MACCATALYST || WINDOWS
			var resolution = uiContext.GetDisplayDensity();
#else
			const float resolution = 1.0f;
#endif
			CompleteLoad(result, token, result?.IsResolutionDependent ?? false, resolution);
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

		public void CompleteLoad(IDisposable? result, CancellationToken token) =>
			CompleteLoad(result, token, false, 1.0f);

		void CompleteLoad(IDisposable? result, CancellationToken token, bool isResolutionDependent, float resolution)
		{
			CancellationTokenSource? cancellation = null;
			lock (_loadLock)
			{
				if (_sourceCancellation is { } current && current.Token == token)
				{
					cancellation = current;
					_sourceCancellation = null;
					_sourceResult = result;
					IsResolutionDependent = isResolutionDependent;
					CurrentResolution = resolution;
				}
			}

			if (cancellation is null)
			{
				result?.Dispose();
				return;
			}

			cancellation.Dispose();
		}

		public void Reset()
		{
			var token = BeginLoad();
			CompleteLoad(null, token);
		}
	}
}