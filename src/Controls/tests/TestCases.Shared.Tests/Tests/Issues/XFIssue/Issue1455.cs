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
	//	App.WaitForElement("Cell 1");
	//	App.Screenshot("I am at Issue 1455");
	//	App.TouchAndHold(q => q.Marked("Cell 4"));
	//	App.Screenshot("Long Press Cell 4 to show context actions");
	//	App.Tap(q => q.Marked("Cell 5"));
	//	App.Screenshot("Clicked another cell and changed menu items");

	//	Assert.AreEqual(1, App.Query(c => c.Marked("Hendrerit")).Length);

	//	App.Back();

	//	App.WaitForElement("Toggle LegacyMode");
	//	App.Tap(q => q.Marked("Toggle LegacyMode"));

	//	App.TouchAndHold(q => q.Marked("Cell 4"));
	//	App.Screenshot("Long Press Cell 4 to show context actions");
	//	App.Tap(q => q.Marked("Cell 5"));
	//	App.Screenshot("Clicked another cell and changed menu items");

	//	Assert.AreEqual(1, App.Query(c => c.Marked("Vestibulum")).Length);
	//}
}