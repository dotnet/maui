using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8305_Toolbar : _IssuesUITest
{
	public override string Issue => "ToolbarItem Badge Support";

	public Issue8305_Toolbar(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemBadgesDisplay()
	{
		// Wait for the page to load
		App.WaitForElement("StatusLabel");

		// Verify toolbar items are present
		App.WaitForElement("BadgeTextItem");

		// Take a screenshot showing badges
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemBadgeIncrements()
	{
		App.WaitForElement("IncrementButton");

		// Increment the count badge
		App.Tap("IncrementButton");

		// Verify the status label updated
		var text = App.FindElement("StatusLabel").GetText();
		Assert.That(text, Is.EqualTo("Count badge: 4"));
	}

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemBadgesClear()
	{
		App.WaitForElement("ClearBadgesButton");

		// Clear all badges
		App.Tap("ClearBadgesButton");

		// Verify status
		var text = App.FindElement("StatusLabel").GetText();
		Assert.That(text, Is.EqualTo("All badges cleared"));

		// Take a screenshot showing no badges
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemBadgeColorChanges()
	{
		App.WaitForElement("SetRedColorButton");

		// Change badge color to red
		App.Tap("SetRedColorButton");

		// Verify status
		var text = App.FindElement("StatusLabel").GetText();
		Assert.That(text, Is.EqualTo("Badge color: Red"));

		// Take a screenshot showing red badge color
		VerifyScreenshot();
	}
}
