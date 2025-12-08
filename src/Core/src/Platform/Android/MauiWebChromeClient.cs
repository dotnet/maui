#nullable disable
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Extensions.Logging;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Platform
{
	public class MauiWebChromeClient : WebChromeClient
	{
		WeakReference<Activity> _activityRef;
		List<int> _requestCodes;
		View _customView;
		ICustomViewCallback _videoViewCallback;
		ScreenOrientation _originalOrientation;
		WindowInsetsControllerCompat _windowInsetsController;
		bool _isSystemBarVisible;

		public MauiWebChromeClient(IWebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));

			_originalOrientation = ScreenOrientation.Unspecified;

			SetContext(handler);
		}

		public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
		{
			base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
			return ChooseFile(filePathCallback, fileChooserParams.CreateIntent(), fileChooserParams.Title);
		}

		public void UnregisterCallbacks()
		{
			if (_requestCodes is null || _requestCodes.Count == 0 || !_activityRef.TryGetTarget(out Activity _))
				return;

			foreach (int requestCode in _requestCodes)
			{
				ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			}

			_requestCodes = null;
		}

		protected bool ChooseFile(IValueCallback filePathCallback, Intent intent, string title)
		{
			if (!_activityRef.TryGetTarget(out Activity activity))
				return false;

			Action<Result, Intent> callback = (resultCode, intentData) =>
			{
				if (filePathCallback is null)
					return;

				Object result = ParseResult(resultCode, intentData);
				filePathCallback.OnReceiveValue(result);
			};

			_requestCodes ??= new List<int>();

			int newRequestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback(callback);

			_requestCodes.Add(newRequestCode);

			activity.StartActivityForResult(Intent.CreateChooser(intent, title), newRequestCode);

			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				UnregisterCallbacks();

			base.Dispose(disposing);
		}

		// OnShowCustomView operate the perform call back to video view functionality
		// is visible in the view.
		public override void OnShowCustomView(View view, ICustomViewCallback callback)
		{
			if (_customView is not null)
			{
				OnHideCustomView();
				return;
			}

			_activityRef.TryGetTarget(out Activity context);
			if (context is null)
				return;

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


		// OnHideCustomView is the WebView call back when the load view in full screen
		// and hide the custom container view.
		public override void OnHideCustomView()
		{
			_activityRef.TryGetTarget(out Activity context);

			if (context is null)
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

		internal void Disconnect()
		{
			UnregisterCallbacks();
			_activityRef = null;
		}

		protected virtual Object ParseResult(Result resultCode, Intent data)
		{
			return FileChooserParams.ParseResult((int)resultCode, data);
		}

		void SetContext(IWebViewHandler handler)
		{
			var activity = (handler?.MauiContext?.Context?.GetActivity()) ?? ApplicationModel.Platform.CurrentActivity;

			if (activity is null)
				handler?.MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning($"Failed to set the activity of the WebChromeClient, can't show pickers on the Webview");

			_activityRef = new WeakReference<Activity>(activity);
		}
	}
}