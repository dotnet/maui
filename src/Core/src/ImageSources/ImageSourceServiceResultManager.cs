#nullable enable
using System;
using System.Collections.Generic;
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
		CancellationTokenSource? _sourceCancellation;
		IDisposable? _sourceResult;
		List<IDisposable>? _supersededResults;
		readonly object _lock = new();

		public bool IsLoading => _sourceCancellation is not null;
		public CancellationToken Token =>
			_sourceCancellation?.Token ?? default;

		public bool IsResolutionDependent { get; private set; }

		public float CurrentResolution { get; private set; } = 1.0f;

		public CancellationToken BeginLoad()
		{
			IDisposable? sourceResult;
			List<IDisposable>? supersededResults;
			CancellationTokenSource? previousCancellation;
			CancellationToken token;

			lock (_lock)
			{
				sourceResult = _sourceResult;
				_sourceResult = null;
				supersededResults = _supersededResults;
				_supersededResults = null;
				previousCancellation = _sourceCancellation;
				_sourceCancellation = new CancellationTokenSource();
				token = _sourceCancellation.Token;
			}

			sourceResult?.Dispose();

			if (supersededResults is not null)
			{
				foreach (var supersededResult in supersededResults)
				{
					supersededResult.Dispose();
				}
			}

			previousCancellation?.Cancel();

			return token;
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

		public void CompleteLoad(IDisposable? result, CancellationToken token)
		{
			lock (_lock)
			{
				if (_sourceCancellation is null || token != _sourceCancellation.Token)
				{
					if (result is not null)
					{
						(_supersededResults ??= new List<IDisposable>()).Add(result);
					}

					return;
				}

				CompleteLoad(result);
			}
		}

		public void CompleteLoad(IDisposable? result)
		{
			CancellationTokenSource? sourceCancellation;

			lock (_lock)
			{
				_sourceResult = result;
				sourceCancellation = _sourceCancellation;
				_sourceCancellation = null;
			}

			sourceCancellation?.Dispose();

			IsResolutionDependent = false;
			CurrentResolution = 1.0f;
		}

		public void Reset()
		{
			var token = BeginLoad();
			CompleteLoad(null, token);
		}
	}
}