using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4720 : _IssuesUITest
{
	public Issue4720(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "UWP: Webview: Memory Leak in WebView";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewDoesntCrashWhenLoadingAHeavyPageAndUsingExecutionModeSeparateProcess()
	{
		//4 iterations were enough to run out of memory before the fix.
		int iterations = 10;

		for (int n = 0; n < iterations; n++)
		{
			App.WaitForElement("NewPage");
			App.Tap("NewPage");
			App.WaitForElement("ClosePage");
			App.Tap("ClosePage");
		}
		App.Tap("GC");
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