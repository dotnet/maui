using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57749 : _IssuesUITest
{
	public Bugzilla57749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "After enabling a disabled button it is not clickable";

	[Test]
	[Category(UITestCategories.Button)]
	[FailsOnIOS]
	public async Task Bugzilla57749Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		await Task.Delay(500);
		App.Tap(("btnClick"));

		var text = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Button was clicked" + "']"));
		ClassicAssert.NotNull(text);

		var button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Ok" + "']"));
		button.Click();
	}
}