using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33287 : _IssuesUITest
{
	public override string Issue => "DisplayAlertAsync throws NullReferenceException when page is no longer displayed";

	public Issue33287(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Page)]
	public void DisplayAlertAsyncShouldNotCrashWhenPageUnloaded()
	{
		App.WaitForElement("NavigateButton");

		// Navigate to second page
		App.Tap("NavigateButton");

		// Wait for second page to appear
		App.WaitForElement("GoBackButton");

		// Go back before the delayed DisplayAlertAsync completes
		App.Tap("GoBackButton");

		// Wait for the delayed DisplayAlertAsync to resolve and update the status label.
		// Without the fix the app crashes (NRE), so the status label is never updated.
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "✅", timeout: TimeSpan.FromSeconds(10)), Is.True,
			"App should show success status after DisplayAlertAsync completes on an unloaded page");
	}
}
