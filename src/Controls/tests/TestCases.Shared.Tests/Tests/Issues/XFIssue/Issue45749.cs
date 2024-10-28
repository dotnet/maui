using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue45749 : _IssuesUITest
{
	public Issue45749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Disable horizontal scroll in the custom listview in android";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnMauiIOS]
	//public void DisableScrollingOnCustomHorizontalListView()
	//{
	//	App.WaitForElement("Button");
	//	App.WaitForElement(q => q.Marked("True"), timeout: TimeSpan.FromSeconds(2));
	//	App.Screenshot("Custom HorizontalListView Scrolling Enabled Default");
	//	App.Tap(q => q.Marked("Toggle ScrollView.IsEnabled"));
	//	App.WaitForElement(q => q.Marked("False"), timeout: TimeSpan.FromSeconds(2));
	//	App.Screenshot("Custom HorizontalListView Scrolling Disabled");
	//}
}