using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28303 : _IssuesUITest
{
	public override string Issue => "[Windows] WebView Navigated event called after cancelling it";

	public Issue28303(TestDevice device)
	: base(device)
	{ }

	[Fact]
	[Trait("Category", UITestCategories.WebView)]
	public void VerifyWebViewNavigatedEventTriggered()
	{
		VerifyInternetConnectivity();
		var navigatedLabel = App.WaitForElement("navigatedLabel");
		Assert.That(navigatedLabel.GetText(), Is.EqualTo("WebView Navigated event is not triggered"));
	}
}