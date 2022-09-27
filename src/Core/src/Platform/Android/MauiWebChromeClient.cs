#nullable disable
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Platform
{
	public class MauiWebChromeClient : WebChromeClient
	{
		WeakReference<Activity> _activityRef;
		List<int> _requestCodes;

		public MauiWebChromeClient(IWebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException("handler");

			SetContext(handler);
		}

		public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
		{
			base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
			return ChooseFile(filePathCallback, fileChooserParams.CreateIntent(), fileChooserParams.Title);
		}

		public void UnregisterCallbacks()
		{
			if (_requestCodes == null || _requestCodes.Count == 0 || !_activityRef.TryGetTarget(out Activity _))
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
				if (filePathCallback == null)
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

			if (activity == null)
				handler?.MauiContext?.CreateLogger<WebViewHandler>()?.LogWarning($"Failed to set the activity of the WebChromeClient, can't show pickers on the Webview");

			_activityRef = new WeakReference<Activity>(activity);
		}
	}
}