using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19542 : _IssuesUITest
{
	public Issue19542(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout item didnot take full width";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemShouldTakeFullWidth()
	{
		// Open the flyout via button and take screenshot
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");

		VerifyScreenshot();
	}
}
