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
	//	App.WaitForElement("ListViewId", "Timed out waiting for the ListView.");

	//	App.Tap(x => x.Marked("G1I1"));
	//	App.Tap(x => x.Marked("G1I2"));
	//	App.Tap(x => x.Marked("G1I3"));
	//	App.Tap(x => x.Marked("G1I4"));
	//	App.Tap(x => x.Marked("G2I1"));
	//	App.Tap(x => x.Marked("G2I2"));

	//	App.WaitForElement("ResultId");
	//	Assert.AreEqual("0 groups", App.WaitForElement("ResultId")[0].ReadText());
	//}
}