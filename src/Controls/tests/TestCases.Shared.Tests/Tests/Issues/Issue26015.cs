using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26015 : _IssuesUITest
{

	public Issue26015(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "MAUI Grouped ListView Remove Item Causes Error when more than one Group item exists";

	[Test]
	[Category(UITestCategories.ListView)]
	public void AppShouldNotCrashWhenRemovingItemsFromGroupedList()
	{
		App.WaitForElement("Test1");
		App.Click("Test1");
		App.Click("Test2");
		App.Click("Test3");
		App.Click("Test4");
	}
}