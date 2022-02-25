
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Webkit;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	class BlazorWebChromeClient : WebChromeClient
	{
		public override bool OnCreateWindow(Android.Webkit.WebView? view, bool isDialog, bool isUserGesture, Message? resultMsg)
		{
			if (view?.Context is not null)
			{
				// Intercept _blank target <a> tags to always open in device browser
				// regardless of ExternalLinkMode.OpenInWebview
				var requestUrl = view.GetHitTestResult().Extra;
				var intent = new Intent(Intent.ActionView, Uri.Parse(requestUrl));
				view.Context.StartActivity(intent);
			}

			// We don't actually want to create a new WebView window so we just return false 
			return false;
		}
	}
}
