using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue99999 : _IssuesUITest
{
	public override string Issue => "Shell CoordinatorLayout 0x0 after repeated GoToAsync navigation cycles on Android";

	public Issue99999(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellPageHasLayoutAfterRepeatedNavigationCycles()
	{
		// Wait for the main page
		App.WaitForElement("RunCyclesButton");

		// Run 10 cycles of: push/pop + tab switch + tab switch back
		// This exercises ShellSectionRenderer.OnHiddenChanged which is the suspected root cause
		App.Tap("RunCyclesButton");

		// Wait for cycles to complete
		App.WaitForElement("NavigateButton", timeout: TimeSpan.FromSeconds(60));

		// Now navigate to the target page - this is where the bug would manifest
		App.Tap("NavigateButton");
		App.WaitForElement("TargetPageLabel", timeout: TimeSpan.FromSeconds(10));

		// Verify the target page has non-zero dimensions
		var rect = App.WaitForElement("TargetPageLabel").GetRect();
		Assert.That(rect.Width, Is.GreaterThan(0),
			"Target page label width should be > 0 after repeated navigation cycles");
		Assert.That(rect.Height, Is.GreaterThan(0),
			"Target page label height should be > 0 after repeated navigation cycles");

		// Check page size reported by the app
		var sizeText = App.FindElement("SizeInfoLabel").GetText();
		Assert.That(sizeText, Does.Not.Contain("-1"),
			$"Page should have valid dimensions after navigation cycles but got: {sizeText}");
		Assert.That(sizeText, Does.Not.Contain("0x0"),
			$"Page should not have 0x0 dimensions after navigation cycles but got: {sizeText}");
	}
}
