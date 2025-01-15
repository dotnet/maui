using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue6286 : _IssuesUITest
{
	public Issue6286(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ObjectDisposedException in Android WebView.EvaluateJavascriptAsync";

	[Test]
	[Category(UITestCategories.WebView)]
	public void Issue6286_WebView_Test()
	{
		App.QueryUntilPresent(() => App.WaitForElement("success"));
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