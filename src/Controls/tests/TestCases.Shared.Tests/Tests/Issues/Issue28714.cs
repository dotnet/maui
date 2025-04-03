using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28714 : _IssuesUITest
{
	public override string Issue => "[iOS] WebView BackgroundColor is not setting correctly";

	public Issue28714(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyWebViewBackgroundColor()
	{
		VerifyInternetConnectivity();
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}