using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3667 : _IssuesUITest
{
	string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

	public Issue3667(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Enhancement] Add text-transforms to Label";

	[Test]
	[Category(UITestCategories.Label)]
	[FailsOnIOS]
	public void Issue3667Tests()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var text1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + text + "']"));
		ClassicAssert.NotNull(text1);

		var tap1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Change TextTransform" + "']"));
		tap1.Click();

		var text2 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + text + "']"));
		ClassicAssert.NotNull(text2);

		var tap2 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Change TextTransform" + "']"));
		tap2.Click();
		
		var text3 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + text.ToLowerInvariant() + "']"));
		ClassicAssert.NotNull(text3);

		var tap3 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Change TextTransform" + "']"));
		tap3.Click();

		var text4 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + text.ToUpperInvariant() + "']"));
		ClassicAssert.NotNull(text4);

		var tap4 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Change TextTransform" + "']"));
		tap4.Click();

		var text5 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + text + "']"));
		ClassicAssert.NotNull(text5);
	}
}