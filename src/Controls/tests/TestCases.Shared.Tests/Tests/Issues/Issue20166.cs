using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20166 : _IssuesUITest
{
	public Issue20166(TestDevice device)
	: base(device) { }

	public override string Issue => "Custom FlyoutIcon visible although FlyoutBehavior set to disabled";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void ShouldHideCustomFlyoutIconWhenNavigatingToPageWithDisabledFlyout()
	{
		// Click button 1 to switch to the page with disabled flyout
		_ = App.WaitForElement("button1");
		App.Click("button1");

		// 2. Verify that the flyout icon is not rendered.
		VerifyScreenshot();
	}
}