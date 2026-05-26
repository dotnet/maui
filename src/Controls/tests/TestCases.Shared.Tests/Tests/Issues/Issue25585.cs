using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25585 : _IssuesUITest
{
	public Issue25585(TestDevice device) : base(device) { }

	public override string Issue => "App Unresponsive when prompting the user from a new page";
	[Test]
	[Category(UITestCategories.DisplayAlert)]
	public void VerifyDisplayAlertIsShown()
	{
		App.WaitForElement("GoToSecondPage");
		App.TapShellFlyoutIcon();
		App.Tap("Second Page");
#if MACCATALYST
		App.WaitForElement(AppiumQuery.ById($"action-button--{999 - 0}"));
#else
		App.WaitForElement("OK");
#endif
	}
}