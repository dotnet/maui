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

		public bool IsLoading => _sourceCancellation is not null;
		public CancellationToken Token =>
			_sourceCancellation?.Token ?? default;

		public bool IsResolutionDependent { get; private set; }

		public float CurrentResolution { get; private set; } = 1.0f;

		public CancellationToken BeginLoad()
		{
			_sourceResult?.Dispose();

			if (_supersededResults is not null)
			{
				foreach (var supersededResult in _supersededResults)
				{
					supersededResult.Dispose();
				}

				_supersededResults = null;
			}

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

		public void CompleteLoad(IDisposable? result, CancellationToken token)
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