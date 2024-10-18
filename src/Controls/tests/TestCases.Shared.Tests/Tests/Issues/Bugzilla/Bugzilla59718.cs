using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla59718 : _IssuesUITest
{
	const string GoBackButtonId = "GoBackButtonId";
	const string Target1 = "Label with TapGesture Cricket";
	const string Target1b = "Label with TapGesture Cricket Tapped!";
	const string Target2 = "Label with no TapGesture Cricket";
	const string Target3 = "You came here from Cricket.";

	public Bugzilla59718(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Multiple issues with listview and navigation in UWP";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void Bugzilla59718Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}
		
		var target1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{Target1}" + "']"));
		target1.Click();

		var target1b = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{Target1b}" + "']"));
		ClassicAssert.NotNull(target1b);

		var target2 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{Target2}" + "']"));
		target2.Click();

		var target3 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{Target3}" + "']"));
		ClassicAssert.NotNull(target3);

		App.WaitForElement(GoBackButtonId);
		App.Tap(GoBackButtonId);

		var target1Again = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{Target1}" + "']"));
		ClassicAssert.NotNull(target1Again);

	}
}