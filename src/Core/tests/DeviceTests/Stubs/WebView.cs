namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WebViewStub : StubBase, IWebView
	{
		public IWebViewSource Source { get; set; }

		public void GoBack() { }
		public void GoForward() { }
		public void Reload() { }
	}
}