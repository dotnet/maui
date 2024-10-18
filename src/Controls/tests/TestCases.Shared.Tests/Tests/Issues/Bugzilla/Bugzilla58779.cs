using NUnit.Framework;
using OpenQA.Selenium.DevTools;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58779 : _IssuesUITest
{
	const string ButtonId = "button";
	const string CancelId = "cancel";

	public Bugzilla58779(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long";

	[Test]
	[FailsOnIOS]
	[Category(UITestCategories.DisplayAlert)]
	public void Bugzilla58779Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(ButtonId);
		App.Tap(ButtonId);
		App.Screenshot("Check list fits on screen");

		var cancel = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + CancelId + "']"));
		cancel.Click();
	}
}