using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Bugzilla60045 : _IssuesUITest
{
	public const string ClickThis = "Click This";
	public const string Fail = "Fail";

	public Bugzilla60045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView with RecycleElement strategy doesn't handle CanExecute of TextCell Command properly";

	[Test]
	[FailsOnIOS]
	[Category(UITestCategories.ListView)]
	public void CommandDoesNotFire()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var clickThis = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + ClickThis + "']"));
		clickThis.Click();

		App.WaitForNoElement(Fail);
	}
}
