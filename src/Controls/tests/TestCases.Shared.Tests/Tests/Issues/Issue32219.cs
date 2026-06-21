using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32219 : _IssuesUITest
{
	public override string Issue => "[Windows] Current page indicator does not update properly in Shell flyout menu";

	public Issue32219(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyFlyoutSelectedCurrentItem()
	{
		App.WaitForElement("homePageLabel");
		App.TapShellFlyoutIcon();
		App.Tap("Login Page");
		App.WaitForElement("loginButton");
		App.Tap("loginButton");
		App.WaitForElement("homePageLabel");
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}
}
