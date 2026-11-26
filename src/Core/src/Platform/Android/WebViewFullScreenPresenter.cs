#nullable disable
using System;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Core.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Presents the view a web page asks to show in fullscreen (<see cref="WebChromeClient.OnShowCustomView(View, WebChromeClient.ICustomViewCallback)"/>)
	/// over the activity, and restores the activity when the page leaves fullscreen.
	/// Shared by <see cref="MauiWebChromeClient"/> and the BlazorWebView's chrome client, so both enter fullscreen the same way.
	/// </summary>
	internal sealed class WebViewFullScreenPresenter
	{
		View _customView;
		WebChromeClient.ICustomViewCallback _videoViewCallback;
		ScreenOrientation _originalOrientation = ScreenOrientation.Unspecified;
		WindowInsetsControllerCompat _windowInsetsController;

		/// <summary>
		/// The system bars fullscreen hid and leaving it shows again, as <see cref="WindowInsetsCompat.Type"/> flags (the
		/// platform's WindowInsets.Type has the same values). From API 30 only the bars that were visible; below API 30,
		/// where the visibility of each bar is not read, all of them.
		/// </summary>
		int _systemBarsToRestore;

		public bool IsShowing => _customView is not null;

		public void Show(Activity context, View view, WebChromeClient.ICustomViewCallback callback)
		{
			if (context is null || context.IsFinishing || context.IsDestroyed || context.Window?.DecorView.IsAttachedToWindow != true)
			{
				// Nothing can be shown without a window on screen, so tell the page right away; otherwise it believes it is
				// still in fullscreen.
				callback?.OnCustomViewHidden();
				return;
			}

			// Captures the current orientation.
			_originalOrientation = context.RequestedOrientation;

			_videoViewCallback = callback;

			_customView = view;
			_customView.SetBackgroundColor(global::Android.Graphics.Color.White);

			context.RequestedOrientation = ScreenOrientation.Landscape;

			// Handle system UI visibility based on Android version.
			// The window's decor-fits policy stays as the app set it (MauiAppCompatActivity draws edge to edge): the custom
			// view goes on the decor view, outside the content that policy pads, and Android has no getter to put the app's
			// own value back afterwards.
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				var controller = context.Window.InsetsController;
				if (controller != null)
				{
					// Store which system bars are visible, each on its own
					var windowInsets = context.Window.DecorView.RootWindowInsets;
					_systemBarsToRestore = 0;
					foreach (var systemBar in new[] { WindowInsetsCompat.Type.StatusBars(), WindowInsetsCompat.Type.NavigationBars(), WindowInsetsCompat.Type.CaptionBar() })
					{
						if (windowInsets?.IsVisible(systemBar) == true)
						{
							_systemBarsToRestore |= systemBar;
						}
					}

					// Hide system bars
					if (_systemBarsToRestore != 0)
					{
						controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
						controller.Hide(WindowInsetsCompat.Type.SystemBars());
					}
				}
			}
			else if (OperatingSystem.IsAndroidVersionAtLeast(19))
			{
				// For API 19-29, use WindowInsetsControllerCompat from AndroidX
				var decorView = context.Window.DecorView;
				var windowInsetsController = WindowCompat.GetInsetsController(context.Window, decorView);

				// Store current state
				_windowInsetsController = windowInsetsController;
				_systemBarsToRestore = WindowInsetsCompat.Type.SystemBars(); // Assume visible by default

				// Hide system bars
				windowInsetsController.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
				windowInsetsController.Hide(WindowInsetsCompat.Type.SystemBars());
			}

			// Add the CustomView
			if (context.Window.DecorView is FrameLayout layout)
			{
				layout.AddView(_customView, new FrameLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent));
			}
		}

		public void Hide(Activity context)
		{
			// Without a shown view there is nothing to restore, and the stored orientation would be stale.
			if (context is null || !IsShowing)
				return;

			// Remove the CustomView
			if (context.Window.DecorView is FrameLayout layout)
			{
				layout.RemoveView(_customView);
			}

			// Restore original screen orientation
			context.RequestedOrientation = _originalOrientation;

			// Restore system UI visibility based on Android version
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				var controller = context.Window.InsetsController;
				if (controller != null && _systemBarsToRestore != 0)
				{
					controller.Show(_systemBarsToRestore);
				}
			}
			else if (OperatingSystem.IsAndroidVersionAtLeast(19))
			{
				// For API 19-29, use WindowInsetsControllerCompat from AndroidX
				if (_windowInsetsController != null && _systemBarsToRestore != 0)
				{
					_windowInsetsController.Show(_systemBarsToRestore);
				}
			}

			_videoViewCallback?.OnCustomViewHidden();
			_customView = null;
			_videoViewCallback = null;
			_windowInsetsController = null;
			_systemBarsToRestore = 0;
		}
	}
}
