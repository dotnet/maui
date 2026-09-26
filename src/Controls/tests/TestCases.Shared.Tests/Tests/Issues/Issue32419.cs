using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32419 : _IssuesUITest
{
	public override string Issue => "[iOS, macOS] Shell menu and flyout items do not update correctly in RTL mode";

	protected override bool ResetAfterEachTest => true;

	public Issue32419(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutContentAlignedInRTL()
	{
		App.WaitForElement("homePageLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyShellMenuItemsAlignedInRTL()
	{
		App.WaitForElement("homePageLabel");
		App.Tap("FlyoutLockedButton");
		App.WaitForElement("MenuItem 1");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}

}
