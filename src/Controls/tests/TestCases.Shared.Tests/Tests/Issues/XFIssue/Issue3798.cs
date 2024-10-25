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
	//	App.WaitForElement("listViewSeparatorColor");
	//	App.Screenshot("Green ListView Separator");
	//	App.Tap(q => q.Marked("item1"));
	//	App.Screenshot("Red ListView Separator");
	//}
}