using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Webkit;
using System.Maui.Internals;
using Object = Java.Lang.Object;

namespace System.Maui.Platform.Android
{
	public class FormsWebChromeClient : WebChromeClient
	{
		Activity _activity;
		List<int> _requestCodes;

		public override bool OnShowFileChooser(global::Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
		{
			base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
			return ChooseFile(filePathCallback, fileChooserParams.CreateIntent(), fileChooserParams.Title);
		}

		public void UnregisterCallbacks()
		{
			if (_requestCodes == null || _requestCodes.Count == 0 || _activity == null)
				return;

			foreach (int requestCode in _requestCodes)
			{
				ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			}

			_requestCodes = null;
		}

		protected bool ChooseFile(IValueCallback filePathCallback, Intent intent, string title)
		{
			if (_activity == null)
				return false;

			Action<Result, Intent> callback = (resultCode, intentData) =>
			{
				if (filePathCallback == null)
					return;

				Java.Lang.Object result = ParseResult(resultCode, intentData);
				filePathCallback.OnReceiveValue(result);
			};

			_requestCodes = _requestCodes ?? new List<int>();

			int newRequestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback(callback);

			_requestCodes.Add(newRequestCode);

			_activity.StartActivityForResult(Intent.CreateChooser(intent, title), newRequestCode);

			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				UnregisterCallbacks();
			base.Dispose(disposing);
		}

		protected virtual Java.Lang.Object ParseResult(Result resultCode, Intent data)
		{
			return FileChooserParams.ParseResult((int)resultCode, data);
		}

		internal void SetContext(Context thisActivity)
		{
			_activity = thisActivity as Activity;
			if (_activity == null)
				Log.Warning(nameof(WebViewRenderer), $"Failed to set the activity of the WebChromeClient, can't show pickers on the Webview");
		}
	}
}