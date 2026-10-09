using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32995 : _IssuesUITest
{
	public Issue32995(TestDevice device) : base(device) { }

	public override string Issue => "TabBarDisabledColor not applied to disabled tabs on iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarDisabledColorAppliedToDisabledTab()
	{
		App.WaitForElement("Tab2");
		VerifyScreenshot("DisabledTabWithGreenColor");

		App.Tap("EnableButton");
		App.WaitForElement("Tab2");
		VerifyScreenshot("EnabledTabWithNormalColor");
	}

	// Regression test for Windows: verifies that a tab added dynamically after appearance
	// is already applied correctly inherits TabBarDisabledColor. Without the fix in
	// ShellItemHandler.SetValues (setting DisabledForeground before IsEnabled), the new
	// tab's NavigationViewItemViewModel.UpdateForeground() fires with DisabledForeground=null
	// and falls back to the default unselected color instead of the custom disabled color.
#if WINDOWS
	[Test]
	[Category(UITestCategories.Shell)]
	public void DynamicallyAddedDisabledTabInheritsTabBarDisabledColor()
	{
		App.WaitForElement("AddDisabledTabButton");
		App.Tap("AddDisabledTabButton");

		// Wait for the new tab to appear in the navigation bar
		App.WaitForElement("Tab3");

		// The dynamically added Tab3 is disabled with TabBarDisabledColor = Green.
		// Without the fix, DisabledForeground is null when IsEnabled.set fires
		// UpdateForeground(), so Tab3 renders with the default unselected color instead of green.
		VerifyScreenshot("DynamicallyAddedDisabledTabWithGreenColor");
	}
#endif
}
