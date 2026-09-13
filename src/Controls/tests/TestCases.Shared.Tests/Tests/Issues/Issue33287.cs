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

		// Navigate to the second page.
		App.Tap("NavigateButton");

		// Wait for second page to appear, then go back immediately
		App.WaitForElement("GoBackButton");
		App.Tap("GoBackButton");

		// Back on the main page, wait until the detached page creates the alert request.
		// Without the fix the synchronous NRE crashes the app before this status is updated.
		App.WaitForElement("MainPageLabel");
		Assert.That(
			App.WaitForTextToBePresentInElement(
				"AlertStatusLabel",
				"Alert request returned",
				timeout: TimeSpan.FromSeconds(10)),
			Is.True,
			"The detached page should create the alert request without crashing");

		// Verify the app is still alive and responsive after the alert request on the detached page.
		// Without the fix the app process is dead and this call will throw/timeout.
		Assert.That(App.FindElement("MainPageLabel").GetText(), Is.EqualTo("MainPage"),
			"App should remain responsive after DisplayAlertAsync on an unloaded page");
	}
}
