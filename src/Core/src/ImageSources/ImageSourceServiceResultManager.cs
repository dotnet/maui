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

		/// <summary>
		/// Records the result of the load that is in progress as soon as its image is shown, so that a load that is
		/// started before it completes releases (or keeps) it.
		/// </summary>
		public void SetResult(IDisposable result) =>
			_sourceResult = result;

		/// <summary>
		/// Takes the result of the completed load, so that starting the next load does not release it.
		/// </summary>
		public IDisposable? DetachResult()
		{
			var result = _sourceResult;
			_sourceResult = null;
			return result;
		}

		/// <summary>
		/// Replaces the result of the completed load with another image of the same source (for example decoded at a larger size).
		/// </summary>
		public void ReplaceResult(IDisposable result)
		{
			var previous = _sourceResult;
			_sourceResult = result;
			previous?.Dispose();
		}

		public void Reset()
		{
			BeginLoad();
			CompleteLoad(null);
		}
	}
}