using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3019 : _IssuesUITest
{
	public Issue3019(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Grouped ListView Header empty for adding items";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void MakeSureListGroupShowsUpAndItemsAreClickable()
	// {
	// 	App.WaitForElement("Group 1");

	// 	App.Tap(x => x.Marked("Grouped Item: 0"));
	// 	App.Tap(x => x.Marked("Grouped Item: 1"));
	// 	App.Tap(x => x.Marked("Grouped Item: 1 Clicked"));

	// }
}
