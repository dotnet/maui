using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21631 : _IssuesUITest
	{
		public Issue21631(TestDevice device) : base(device) { }

		public override string Issue => 
			"Injecting base tag in Webview2 works";

		[Test]
		public async Task NavigateToStringWithWebviewWorks()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.iOS });

			App.WaitForElement("WaitForWebView");
			await Task.Delay(500);
			VerifyScreenshot();
		}
	}
}