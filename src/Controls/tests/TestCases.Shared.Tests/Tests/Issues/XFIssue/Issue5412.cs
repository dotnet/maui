using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5412 : _IssuesUITest
{
	public Issue5412(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "5412 - (NavigationBar disappears on FlyoutPage)";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Issue5412Test()
	{
		App.WaitForElementTillPageNavigationSettled("Index Page");
		App.TapFlyoutPageIcon("Menu title");
		App.WaitForElementTillPageNavigationSettled("Settings");
		App.Tap("Settings");
		App.WaitForElementTillPageNavigationSettled("Settings Page");
		App.TapBackArrow();

		// This fails if the menu isn't displayed (original error behavior)
		App.WaitForElementTillPageNavigationSettled("Index Page");
		App.WaitForFlyoutIcon("Menu title", isShell: false);
	}
}