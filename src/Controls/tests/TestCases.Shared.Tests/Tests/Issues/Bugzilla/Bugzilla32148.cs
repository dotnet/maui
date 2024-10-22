using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32148 : _IssuesUITest
{
	public Bugzilla32148(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => " Pull to refresh hides the first item on a list view";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnApple]
	[FailsOnWindows("Sometimes the Teardown process fails after running the test.")]
	public void Bugzilla32148Test()
	{
		App.WaitForNoElement("Contact0 LastName");
		App.Tap(AppiumQuery.ByXPath("//*[@text='" + "Search" + "']"));
		App.WaitForNoElement("Contact0 LastName");
		App.Screenshot("For manual review, verify that the first cell is visible");
	}
}
