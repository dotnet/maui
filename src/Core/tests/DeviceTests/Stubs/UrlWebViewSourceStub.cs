namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class UrlWebViewSourceStub : IWebViewSource
	{
		public string Url { get; set; }

		public void Load(IWebViewDelegate webViewDelegate)
		{
			webViewDelegate.LoadUrl(Url);
		}
	}
}