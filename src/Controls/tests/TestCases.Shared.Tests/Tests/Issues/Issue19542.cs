using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19542 : _IssuesUITest
{
	public Issue19542(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout item didnt take full width";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemShouldTakeFullWidth()
	{
		// Wait for the main page content to confirm navigation succeeded
		App.WaitForElement("Label19542");

		// Get the bounds of the flyout item's grid (LightGreen background)
		// and verify it spans the full pane width by comparing to a known element
		var itemRect = App.WaitForElement("FlyoutItemGrid").GetRect();
		var labelRect = App.WaitForElement("Label19542").GetRect();

		// The flyout item grid must be at x=0 and have a non-trivial width
		Assert.That(itemRect.X, Is.EqualTo(0).Within(5), "Flyout item grid should start at x=0");
		Assert.That(itemRect.Width, Is.GreaterThan(labelRect.Width),
			"Flyout item grid should be wider than the page content label (i.e. it fills the pane)");
	}
}
