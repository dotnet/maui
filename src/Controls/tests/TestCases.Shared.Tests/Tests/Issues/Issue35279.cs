#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class Issue35279 : _IssuesUITest
{
	public override string Issue =>
		"MenuBarItem named 'Edit' (or any system menu title) should merge its items into the existing system menu on Mac Catalyst";

	public Issue35279(TestDevice testDevice) : base(testDevice) { }

	// Verifies that a MenuFlyoutItem added to a MenuBarItem whose Text matches a system
	// menu title ("Edit") is visible in that system menu on Mac Catalyst.
	// Regression: before the fix, the MAUI "Edit" menu was created as a duplicate
	// sibling that Mac Catalyst silently hid, so "Copy" never appeared.
	[Test]
	public void EditMenuCopyItemIsVisible()
	{
		App.WaitForElement("ResultLabel");

		// Open the Edit menu from the Mac menu bar and tap the Copy item.
		App.WaitForElement("Edit");
		App.Tap("Edit");
		App.WaitForElement("Copy");
		App.Tap("Copy");

		Assert.That(
			App.WaitForElement("ResultLabel").GetText(),
			Is.EqualTo("Copy executed"));
	}
}
#endif
