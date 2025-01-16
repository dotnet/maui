#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // ListView Group Header items are invisible while collapsing, Issue: https://github.com/dotnet/maui/issues/26494. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ListView)]
public class Bugzilla40704 : _IssuesUITest
{
	const string CollapseButton = "btnCollapse";
	const string List = "lstMain";
	public Bugzilla40704(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Strange duplication of listview headers when collapsing/expanding sections";

	[Test]
	public void Bugzilla40704HeaderPresentTest()
	{
		App.WaitForElement("Menu - 0");
	}


	[Test]
	public void Bugzilla40704Test()
	{
		App.ScrollDown(List, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(CollapseButton);
		App.Tap(CollapseButton);

		App.ScrollDown(List, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(CollapseButton);
		App.Tap(CollapseButton);

		App.ScrollDown(List, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(CollapseButton);
		App.Tap(CollapseButton);

		App.WaitForElement("Menu - 2");
		App.WaitForElement("Menu - 1");
		App.WaitForElement("Menu - 0");
	}
}
#endif