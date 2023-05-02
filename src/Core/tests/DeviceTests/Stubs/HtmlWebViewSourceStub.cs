namespace Microsoft.Maui.DeviceTests.Stubs
{
	class HtmlWebViewSourceStub : IWebViewSource
	{
		public string Html { get; set; }

		public void Load(IWebViewDelegate webViewDelegate)
		{
			webViewDelegate.LoadHtml(Html, null);
		}
	}
}
