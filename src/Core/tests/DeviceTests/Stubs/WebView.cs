using System;
using System.Net;
using System.Threading.Tasks;


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

	}
}