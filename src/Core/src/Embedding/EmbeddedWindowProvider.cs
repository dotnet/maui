#if ANDROID || IOS || MACCATALYST || WINDOWS
using System;

#if ANDROID
using PlatformWindow = Android.App.Activity;
#elif IOS || MACCATALYST
using PlatformWindow = UIKit.UIWindow;
#elif WINDOWS
using PlatformWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Embedding;

/// <summary>
/// A proxy service that provides access to the embedded window and its associated platform window.
/// </summary>
/// <remarks>
/// This is used when embedding but also serves to allow the platform window to be added to the service collection
/// before it is actually known or instantiated.
/// </remarks>
internal class EmbeddedWindowProvider
{
	WeakReference<PlatformWindow>? _platformWindow;
	WeakReference<IWindow>? _window;

	public void SetWindow(PlatformWindow? platformWindow, IWindow? window)
	{
		_platformWindow = platformWindow is null ? null : new WeakReference<PlatformWindow>(platformWindow);
		_window = window is null ? null : new WeakReference<IWindow>(window);
	}

	public PlatformWindow? PlatformWindow => _platformWindow?.GetTargetOrDefault();

	public IWindow? Window => _window?.GetTargetOrDefault();
}
#endif
