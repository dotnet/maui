using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20627 : _IssuesUITest
	{
		public Issue20627(TestDevice device) : base(device)
		{
		}

		public override string Issue => "WebView error if we leave page while it's navigating";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewNoCrashLeavingPopup()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			// 1. Open a Popup with a WebView.
			App.WaitForElement("TestOpenPopupButton");
			App.Click("TestOpenPopupButton");

			// 2. Close the Popup.
			App.WaitForElement("TestPopupLabel");
			App.Click("TestClosePopupButton");

			// 3. Without crashes, the test has passed.
		}
	}
}
