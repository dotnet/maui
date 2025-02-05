#if TEST_FAILS_ON_IOS // On iOS the app get freez and getting an exception 'System.IO.FileNotFoundException' in System.Private.CoreLib.dll while using the images from url in HostApp. But in simple sample it works fine.
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

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue11381RemoveListViewGroups()
	{
		App.WaitForElement("ListViewId", "Timed out waiting for the ListView.");
		App.Tap("G2I2");
		App.Tap("G2I1");
		App.Tap("G1I4");
		App.Tap("G1I3");
		App.Tap("G1I2");
		App.Tap("G1I1");
		App.WaitForElement("ResultId");
		Assert.That("0 groups", Is.EqualTo(App.WaitForElement("ResultId").ReadText()));
	}
}
#endif