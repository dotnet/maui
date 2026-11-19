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
		bool _isSystemBarVisible;

		public bool IsShowing => _customView is not null;

		public void Show(Activity context, View view, WebChromeClient.ICustomViewCallback callback)
		{
			if (context is null)
			{
				// Nothing can be shown, so tell the page right away; otherwise it believes it is still in fullscreen.
				callback?.OnCustomViewHidden();
				return;
			}

			// Captures the current orientation.
			_originalOrientation = context.RequestedOrientation;

			_videoViewCallback = callback;

			_customView = view;
			_customView.SetBackgroundColor(global::Android.Graphics.Color.White);

			context.RequestedOrientation = ScreenOrientation.Landscape;

			// Handle system UI visibility based on Android version
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				WindowCompat.SetDecorFitsSystemWindows(context.Window, false);

				var controller = context.Window.InsetsController;
				if (controller != null)
				{
					// Store current visibility state
					var windowInsets = context.Window.DecorView.RootWindowInsets;
					_isSystemBarVisible = windowInsets.IsVisible(WindowInsetsCompat.Type.NavigationBars()) ||
						windowInsets.IsVisible(WindowInsetsCompat.Type.StatusBars());

					// Hide system bars
					if (_isSystemBarVisible)
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
				_isSystemBarVisible = true; // Assume visible by default

				// Hide system bars
				windowInsetsController.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
				windowInsetsController.Hide(WindowInsetsCompat.Type.SystemBars());

				// Make content appear behind system bars
				WindowCompat.SetDecorFitsSystemWindows(context.Window, false);
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
				WindowCompat.SetDecorFitsSystemWindows(context.Window, true);

				var controller = context.Window.InsetsController;
				if (controller != null && _isSystemBarVisible)
				{
					controller.Show(WindowInsetsCompat.Type.SystemBars());
				}
			}
			else if (OperatingSystem.IsAndroidVersionAtLeast(19))
			{
				// For API 19-29, use WindowInsetsControllerCompat from AndroidX
				if (_windowInsetsController != null && _isSystemBarVisible)
				{
					_windowInsetsController.Show(WindowInsetsCompat.Type.SystemBars());
				}

				// Restore content layout
				if (context.Window != null)
				{
					WindowCompat.SetDecorFitsSystemWindows(context.Window, true);
				}
			}

			_videoViewCallback?.OnCustomViewHidden();
			_customView = null;
			_videoViewCallback = null;
			_windowInsetsController = null;
		}
	}
}
