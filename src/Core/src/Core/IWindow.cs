using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow : ITitledElement
	{
		/// <summary>
		/// Gets the current Page displayed in the Window.
		/// </summary>
		IView? Content { get; }

		/// <summary>
		/// Gets the current visual diagnostics overlay for the Window.
		/// </summary>
		IVisualDiagnosticsOverlay VisualDiagnosticsOverlay { get; }

		/// <summary>
		/// Gets the read only collection of Window Overlays on top of the Window.
		/// </summary>
		IReadOnlyCollection<IWindowOverlay> Overlays { get; }

		/// <summary>
		/// Gets the specified X coordinate of the IWindow.
		/// </summary>
		double X { get; }

		/// <summary>
		/// Gets the specified Y coordinate of the IWindow.
		/// </summary>
		double Y { get; }

		/// <summary>
		/// Gets the specified width of the IWindow.
		/// </summary>
		double Width { get; }

		/// <summary>
		/// Gets the specified minimum width constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumWidth { get; }

		/// <summary>
		/// Gets the specified maximum width constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumWidth { get; }

		/// <summary>
		/// Gets the specified height of the IWindow.
		/// </summary>
		double Height { get; }

		/// <summary>
		/// Gets the specified minimum height constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumHeight { get; }

		/// <summary>
		/// Gets the specified maximum height constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumHeight { get; }

		/// <summary>
		/// Direction in which the UI elements are scanned by the eye.
		/// </summary>
		FlowDirection FlowDirection { get; }

		/// <summary>
		/// Adds a Window Overlay to the current Window.
		/// </summary>
		/// <param name="overlay"><see cref="IWindowOverlay"/>.</param>
		/// <returns>Boolean if the window overlay was added.</returns>
		bool AddOverlay(IWindowOverlay overlay);

		/// <summary>
		/// Removes a Window Overlay to the current Window.
		/// </summary>
		/// <param name="overlay"><see cref="IWindowOverlay"/>.</param>
		/// <returns>Boolean if the window overlay was removed.</returns>
		bool RemoveOverlay(IWindowOverlay overlay);

		/// <summary>
		/// Occurs after the native window has been created. The cross-platform window has a native window handler at this point, but the window might not be visible yet.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnPostCreate</c> on Android, <c>UIApplicationDelegate.FinishedLaunching</c> on iOS and Mac Catalyst (<c>SceneWillConnect</c> when using the scene lifecycle), and <c>MauiWinUIWindow.Created</c> on Windows.
		/// </remarks>
		void Created();

		/// <summary>
		/// Occurs when the app is resumed after having been stopped. This is not raised when the app first launches; it is only raised after <see cref="Stopped"/>.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnRestart</c> on Android (which likewise only fires after <c>Activity.OnStop</c>), <c>UIApplicationDelegate.WillEnterForeground</c> on iOS and Mac Catalyst (<c>SceneWillEnterForeground</c> under the scene lifecycle), and <c>MauiWinUIWindow.Resumed</c> on Windows.
		/// </remarks>
		void Resumed();

		/// <summary>
		/// Occurs when the window has been activated and is, or will become, the focused window.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnResume</c> on Android (despite the name, Android's <c>OnResume</c> maps to this event, not to <see cref="Resumed"/>), <c>UIApplicationDelegate.OnActivated</c> on iOS and Mac Catalyst (<c>SceneOnActivated</c> under the scene lifecycle), and <c>Window.Activated</c> with <c>CodeActivated</c> or <c>PointerActivated</c> on Windows.
		/// </remarks>
		void Activated();

		/// <summary>
		/// Occurs when the window is no longer the focused window. The window might still be visible, for example behind a permission dialog on Android.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnPause</c> on Android, <c>UIApplicationDelegate.OnResignActivation</c> on iOS and Mac Catalyst (<c>SceneOnResignActivation</c> under the scene lifecycle), and <c>Window.Activated</c> with the <c>Deactivated</c> state on Windows, where activation and deactivation are the same native event distinguished by its state argument.
		/// </remarks>
		void Deactivated();

		/// <summary>
		/// Occurs when the window is no longer visible. The operating system may terminate the app from this state, so there is no guarantee that <see cref="Resumed"/> will follow.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnStop</c> on Android, <c>UIApplicationDelegate.DidEnterBackground</c> on iOS and Mac Catalyst (<c>SceneDidEnterBackground</c> under the scene lifecycle), and <c>Window.VisibilityChanged</c> on Windows.
		/// </remarks>
		void Stopped();

		/// <summary>
		/// Occurs when the native window is being destroyed and deallocated. The same cross-platform window might be used with a new native window when the app is reopened.
		/// </summary>
		/// <remarks>
		/// Raised from <c>Activity.OnDestroy</c> on Android, <c>UIApplicationDelegate.WillTerminate</c> on iOS and Mac Catalyst (<c>SceneDidDisconnect</c> under the scene lifecycle), and <c>Window.Closed</c> on Windows.
		/// </remarks>
		void Destroying();

		/// <summary>
		/// Occurs when the window is entering a background state. Raised on iOS and Mac Catalyst only; persist any state to <paramref name="state"/> so the operating system can restore it when the window resumes.
		/// </summary>
		void Backgrounding(IPersistedState state);

		/// <summary>
		/// Occurs when the back button is pressed.
		/// </summary>
		/// <returns>Whether or not the back navigation was handled.</returns>
		bool BackButtonClicked();

		void DisplayDensityChanged(float displayDensity);

		void FrameChanged(Rect frame);

		float RequestDisplayDensity();

#if !NETSTANDARD
		/// <summary>
		/// Gets the preferred theme for the status bar area on mobile platforms.
		/// Controls whether OS-drawn icons (clock, battery, signal) are light or dark.
		/// Default follows the current app theme. No-op on desktop platforms.
		/// </summary>
		StatusBarTheme StatusBarTheme => StatusBarTheme.Default;
#endif

#if WINDOWS || MACCATALYST
		ITitleBar? TitleBar => null;
		/// <summary>Gets a value indicating whether the window can be minimized.</summary>
		bool IsMinimizable => true;
		/// <summary>Gets a value indicating whether the window can be maximized.</summary>
		bool IsMaximizable => true;
#endif

#if WINDOWS
		Rect[]? TitleBarDragRectangles => null;
#endif
	}
}
