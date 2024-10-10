using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32040 : _IssuesUITest
{
	public Bugzilla32040(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "EntryCell.Tapped or SwitchCell.Tapped does not fire when within a TableView ";

	[Test]
	[Category(UITestCategories.Cells)]
	[FailsOnAllPlatforms]
	public void TappedWorksForEntryAndSwithCellTest()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForNoElement("blahblah");

		var item1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "blahblah" + "']"));
		item1.Click();

		var item2 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "yaddayadda" + "']"));
		item2.Click();

		Assert.That(App.FindElements("Tapped").Count,
			Is.GreaterThanOrEqualTo(2));
	}
}
