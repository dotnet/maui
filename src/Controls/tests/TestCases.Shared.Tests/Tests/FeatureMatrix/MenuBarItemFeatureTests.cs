using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Visual)]
public class MenuBarItemFeatureTests : UITest
{
	public const string MenuBarItemFeatureMatrix = "MenuBarItem Feature Matrix";

	public MenuBarItemFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(MenuBarItemFeatureMatrix);
	}

	[Test, Order(1)]
	public void MenuBarItem()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FileMenuBarItem");
		App.Tap("FileMenuBarItem");

		App.WaitForElement("ExitMenuFlyoutItem");
		App.Tap("ExitMenuFlyoutItem");
	}

}