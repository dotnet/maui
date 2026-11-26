#nullable disable
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Object = Java.Lang.Object;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Platform
{
	public class MauiWebChromeClient : WebChromeClient
	{
		WeakReference<Activity> _activityRef;
		List<int> _requestCodes;
		readonly WebViewFullScreenPresenter _fullScreenPresenter = new();

		public MauiWebChromeClient(IWebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));

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

				Object result = IsFileChooserResultValid(intentData)
					? ParseResult(resultCode, intentData)
					: null;
				filePathCallback.OnReceiveValue(result);
			};

			_requestCodes ??= new List<int>();

			int newRequestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback(callback);

			_requestCodes.Add(newRequestCode);

			activity.StartActivityForResult(Intent.CreateChooser(intent, title), newRequestCode);

			return true;
		}

		internal static bool IsFileChooserResultValid(Intent intent)
		{
			if (intent is null)
				return true;

			if (!IsFileChooserUriValid(intent.Data))
				return false;

			var clipData = intent.ClipData;
			if (clipData is null)
				return true;

			for (var i = 0; i < clipData.ItemCount; i++)
			{
				if (!IsFileChooserUriValid(clipData.GetItemAt(i)?.Uri))
					return false;
			}

			return true;
		}

		static bool IsFileChooserUriValid(AndroidUri uri)
		{
			if (uri is null || !string.Equals(uri.Scheme, FileSystemUtils.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
				return true;

			return FileSystemUtils.TryGetValidatedPhysicalPath(uri.Path, out _);
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
			if (_fullScreenPresenter.IsShowing)
			{
				OnHideCustomView();
				return;
			}

			_activityRef.TryGetTarget(out Activity context);
			_fullScreenPresenter.Show(context, view, callback);
		}


		// OnHideCustomView is the WebView call back when the load view in full screen
		// and hide the custom container view.
		public override void OnHideCustomView()
		{
			_activityRef.TryGetTarget(out Activity context);
			_fullScreenPresenter.Hide(context);
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