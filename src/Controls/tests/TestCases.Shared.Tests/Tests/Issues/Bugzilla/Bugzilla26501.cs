/*using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla26501 : _IssuesUITest
{
    public Bugzilla26501(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "BindingSource / Context action issue";

	[Test]
	[Category(UITestCategories.InputTransparent)]
	public void TestCellsShowAfterRefresh()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var item = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Refresh" + "']"));
		item.Click();

		App.WaitForNoElement("ZOOMER robothund 2");
	}
}*/
