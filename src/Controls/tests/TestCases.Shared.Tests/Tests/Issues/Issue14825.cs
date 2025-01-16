using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14825 : _IssuesUITest
{
	public override string Issue => "Capture WebView screenshot";

	public Issue14825(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public async Task ValidateWebViewScreenshot()
	{
		App.WaitForElement("TestInstructions");
		await Task.Delay(TimeSpan.FromSeconds(5));
		// Click the capture button to capture a WebView screenshot.
		App.Click("Capture");

		VerifyScreenshot();
	}
	public override void TestSetup()
	{
		base.TestSetup();

		try
		{
			App.WaitForElement("NoInternetAccessLabel", timeout: TimeSpan.FromSeconds(1));
			Assert.Inconclusive("This device doesn't have internet access");
		}
		catch (TimeoutException)
		{
			// Element not found within timeout, assume internet is available
			// Continue with the test
		}
	}
}
