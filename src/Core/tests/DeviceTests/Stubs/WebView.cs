using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

#if ANDROID
using PlatformImage = Android.Graphics.Bitmap;
#elif IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
#elif WINDOWS
using PlatformImage = Microsoft.Graphics.Canvas.CanvasBitmap;
#endif

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WebViewStub : StubBase, IWebView
	{
		public Func<WebNavigationEvent, string, bool> NavigatingDelegate { get; set; }
		public Action<WebNavigationEvent, string, WebNavigationResult> NavigatedDelegate { get; set; }

		public IWebViewSource Source { get; set; }
		public CookieContainer Cookies { get; }
		public bool CanGoBack { get; set; }
		public bool CanGoForward { get; set; }
		public string UserAgent { get; set; }

		public void GoBack() { }
		public void GoForward() { }
		public void Reload() { }
		public void Eval(string script) { }
		public Task<string> EvaluateJavaScriptAsync(string script)
			=> Handler.InvokeAsync(nameof(IWebView.EvaluateJavaScriptAsync), new EvaluateJavaScriptAsyncRequest(script));
		public bool Navigating(WebNavigationEvent evnt, string url)
			=> NavigatingDelegate?.Invoke(evnt, url) ?? false;
		public void Navigated(WebNavigationEvent evnt, string url, WebNavigationResult result)
			=> NavigatedDelegate?.Invoke(evnt, url, result);
		public void ProcessTerminated(WebProcessTerminated webProcessTerminated) { }

#pragma warning disable CS1998 // Ignore method lacks 'await' - Not all platforms need an await
		public async Task<PlatformImage> Capture()
#pragma warning restore CS1998 // Ignore method lacks 'await' - Not all platforms need an await
		{
#if ANDROID
			// While this does capture some content, it will not capture video or canvas objects
			var v = Handler!.PlatformView as Android.Webkit.WebView;
			var bitmap = Android.Graphics.Bitmap.CreateBitmap(v!.Width, v!.Height, Android.Graphics.Bitmap.Config.Argb8888!);
			var canvas = new Android.Graphics.Canvas(bitmap!);
			v.Draw(canvas);
			return bitmap;
#elif IOS || MACCATALYST
			var v = Handler.PlatformView as MauiWKWebView;
			var i = await v.TakeSnapshotAsync(new WebKit.WKSnapshotConfiguration
			{
				Rect = v.Frame,
				AfterScreenUpdates = false
			});
			return i;
#elif WINDOWS
			var v = Handler!.PlatformView as MauiWebView;
			using var ms = new MemoryStream();
			var randomMs = ms.AsRandomAccessStream();
			await v!.CoreWebView2.CapturePreviewAsync(Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png, randomMs);

			var device = Microsoft.Graphics.Canvas.CanvasDevice.GetSharedDevice();
			var cb = await Microsoft.Graphics.Canvas.CanvasBitmap.LoadAsync(device, randomMs);
			return cb;
#endif
		}
	}
}