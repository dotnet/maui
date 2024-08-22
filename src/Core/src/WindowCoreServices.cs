using System;

#if ANDROID
using PlatformWindow = Android.App.Activity;
#elif IOS || MACCATALYST
using PlatformWindow = UIKit.UIWindow;
#elif WINDOWS
using PlatformWindow = Microsoft.UI.Xaml.Window;
#else
using PlatformWindow = System.Object;
#endif

namespace Microsoft.Maui;

/// <summary>
/// A proxy service that provides access to the window and its associated platform window.
/// </summary>
/// <remarks>
/// This is used when embedding but also serves to allow the platform window to be added to the service collection
/// before it is actually known or instantiated.
/// </remarks>
internal class WindowCoreServices : IWindowCoreServices

{
	[ThreadStatic]
	// this is mainly settable for unit testing purposes
	static IWindowCoreServices? s_currentProvider;

	public static IWindowCoreServices Current =>
		s_currentProvider ??= new WindowCoreServices();

	WeakReference<PlatformWindow>? _platformWindow;

	WeakReference<IWindow>? _window;

	internal void SetWindow(PlatformWindow? platformWindow, IWindow? window)
	{
		s_currentProvider ??= this;
		_platformWindow = platformWindow is null ? null : new WeakReference<PlatformWindow>(platformWindow);
		_window = window is null ? null : new WeakReference<IWindow>(window);
	}

	internal PlatformWindow? PlatformWindow => _platformWindow?.GetTargetOrDefault();

	public IWindow? Window => _window?.GetTargetOrDefault();
}
