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

		// Navigate to second page (starts a 2-second delayed DisplayAlertAsync)
		App.Tap("NavigateButton");

		// Wait for second page to appear, then go back immediately
		App.WaitForElement("GoBackButton");
		App.Tap("GoBackButton");

		// Back on main page — wait for the delayed DisplayAlertAsync to fire.
		// Without the fix the NRE crashes the app and this element becomes unreachable.
		App.WaitForElement("MainPageLabel");
		System.Threading.Thread.Sleep(3000);

		// Verify the app is still alive and responsive after the alert fired on the detached page.
		// Without the fix the app process is dead and this call will throw/timeout.
		Assert.That(App.FindElement("MainPageLabel").GetText(), Is.EqualTo("MainPage"),
			"App should remain responsive after DisplayAlertAsync on an unloaded page");
	}
}
