using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue3798 : _IssuesUITest
{
	public Issue3798(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] SeparatorColor of ListView is NOT updated dynamically";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnMauiIOS]
	//public void Issue3798Test()
	//{
	//	RunningApp.WaitForElement("listViewSeparatorColor");
	//	RunningApp.Screenshot("Green ListView Separator");
	//	RunningApp.Tap(q => q.Marked("item1"));
	//	RunningApp.Screenshot("Red ListView Separator");
	//}
}