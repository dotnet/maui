using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8305 : _IssuesUITest
{
	public override string Issue => "Add Shell Badge support";

	public Issue8305(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBadgeInitialBadgeIsVisible()
	{
		// Tab2 ("Messages") is initialized with BadgeText = "3" in the HostApp
		// Verify the page loaded successfully
		App.WaitForElement("HeaderLabel");

		// Take a screenshot to verify the initial badge is visible on the Messages tab
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBadgeCanBeSetAtRuntime()
	{
		App.WaitForElement("SetBadgeButton");

		// Clear the initial badge first
		App.Tap("ClearBadgeButton");

		// Set a new badge at runtime
		App.Tap("SetBadgeButton");

		// Verify the badge appears with the new value
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBadgeCanBeCleared()
	{
		App.WaitForElement("ClearBadgeButton");

		// Clear the badge
		App.Tap("ClearBadgeButton");

		// Verify the badge is gone
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBadgeMultipleTabsCanHaveBadges()
	{
		App.WaitForElement("SetBadgeButton");

		// Set badges on both Tab2 and Tab3
		App.Tap("SetBadgeButton");
		App.Tap("SetTab3BadgeButton");

		// Verify both badges are visible
		VerifyScreenshot();
	}
}
