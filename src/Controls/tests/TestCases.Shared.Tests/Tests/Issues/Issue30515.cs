using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30515 : _IssuesUITest
{
	public override string Issue => "[iOS] WebView.Reload() with HtmlWebViewSource returns WebNavigationResult.Failure in Navigated event";

	public Issue30515(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyWebViewHTMLSourceReloadStatus()
	{
		App.WaitForElement("NavigationStatusLabel");
		App.Tap("ReloadButton");
		Assert.That(App.FindElement("NavigationStatusLabel").GetText(), Is.EqualTo("Success"));
	}
}