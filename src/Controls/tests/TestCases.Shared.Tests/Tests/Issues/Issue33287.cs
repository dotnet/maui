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

		// Go back before the 5-second delay completes
		App.Tap("GoBackButton");

		// Wait for the main page and the delayed DisplayAlertAsync to resolve (~5s + buffer)
		App.WaitForElement("StatusLabel");

		// Assert positive success: the status must contain the success marker.
		// Without the fix the app crashes (NRE), so StatusLabel is never updated.
		var status = App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(10)).GetText();
		int retries = 12;
		while (retries-- > 0 && (status is null || !status.Contains("✅")))
		{
			System.Threading.Thread.Sleep(1000);
			status = App.FindElement("StatusLabel").GetText();
		}

		Assert.That(status, Does.Contain("✅"),
			"App should show success status after DisplayAlertAsync completes on an unloaded page");
	}
}
