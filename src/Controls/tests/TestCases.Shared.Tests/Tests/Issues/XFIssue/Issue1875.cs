using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1875 : _IssuesUITest
{
	public Issue1875(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NSRangeException adding items through ItemAppearing";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void NSRangeException()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var clickThis = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Load" + "']"));
		clickThis.Click();

		var element5 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "5" + "']"));
		ClassicAssert.IsNotNull(element5);
	}
}
