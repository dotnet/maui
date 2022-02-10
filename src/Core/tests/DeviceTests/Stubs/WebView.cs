using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WebViewStub : StubBase, IWebView
	{
		public IWebViewSource Source { get; set; }
		public bool CanGoBack { get; set; }
		public bool CanGoForward { get; set; }

		public void GoBack() { }
		public void GoForward() { }
		public void Reload() { }
		public void Eval(string script) { }
		public Task<string> EvaluateJavaScriptAsync(string script) { return null; }
	}
}
