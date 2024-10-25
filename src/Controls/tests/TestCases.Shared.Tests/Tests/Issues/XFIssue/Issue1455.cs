using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue1455 : _IssuesUITest
{
	public Issue1455(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Context action are not changed when selected item changed on Android";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//public void RefreshContextActions()
	//{
	//	RunningApp.WaitForElement("Cell 1");
	//	RunningApp.Screenshot("I am at Issue 1455");
	//	RunningApp.TouchAndHold(q => q.Marked("Cell 4"));
	//	RunningApp.Screenshot("Long Press Cell 4 to show context actions");
	//	RunningApp.Tap(q => q.Marked("Cell 5"));
	//	RunningApp.Screenshot("Clicked another cell and changed menu items");

	//	Assert.AreEqual(1, RunningApp.Query(c => c.Marked("Hendrerit")).Length);

	//	RunningApp.Back();

	//	RunningApp.WaitForElement("Toggle LegacyMode");
	//	RunningApp.Tap(q => q.Marked("Toggle LegacyMode"));

	//	RunningApp.TouchAndHold(q => q.Marked("Cell 4"));
	//	RunningApp.Screenshot("Long Press Cell 4 to show context actions");
	//	RunningApp.Tap(q => q.Marked("Cell 5"));
	//	RunningApp.Screenshot("Clicked another cell and changed menu items");

	//	Assert.AreEqual(1, RunningApp.Query(c => c.Marked("Vestibulum")).Length);
	//}
}