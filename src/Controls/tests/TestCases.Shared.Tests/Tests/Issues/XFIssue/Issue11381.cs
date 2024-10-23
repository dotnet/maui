using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11381 : _IssuesUITest
{
	public Issue11381(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [iOS] NRE on grouped ListView when removing cells with gesture recognizers";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnMauiIOS]
	//public void Issue11381RemoveListViewGroups()
	//{
	//	RunningApp.WaitForElement("ListViewId", "Timed out waiting for the ListView.");

	//	RunningApp.Tap(x => x.Marked("G1I1"));
	//	RunningApp.Tap(x => x.Marked("G1I2"));
	//	RunningApp.Tap(x => x.Marked("G1I3"));
	//	RunningApp.Tap(x => x.Marked("G1I4"));
	//	RunningApp.Tap(x => x.Marked("G2I1"));
	//	RunningApp.Tap(x => x.Marked("G2I2"));

	//	RunningApp.WaitForElement("ResultId");
	//	Assert.AreEqual("0 groups", RunningApp.WaitForElement("ResultId")[0].ReadText());
	//}
}