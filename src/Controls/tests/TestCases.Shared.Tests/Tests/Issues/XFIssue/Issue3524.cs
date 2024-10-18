using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3524 : _IssuesUITest
{
	const string kText = "Click Me To Increment";

	public Issue3524(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ICommand binding from a TapGestureRecognizer on a Span doesn't work";

	[Test]
	[Category(UITestCategories.Gestures)]
	[FailsOnIOS]
	public void SpanGestureCommand()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var text = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + kText + "']"));
		text.Click();

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{kText}: 1" + "']"));
		ClassicAssert.NotNull(result);
	}
}