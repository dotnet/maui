using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class Issue6784 : _IssuesUITest
{
	public Issue6784(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ShellItem.CurrentItem is not set when selecting shell section aggregated in more tab";

	//[Test]
	//public void CurrentItemIsSetWhenSelectingShellSectionAggregatedInMoreTab()
	//{
	//	App.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	App.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	App.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
	//	App.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	App.WaitForElement(x => x.Text("Success"));
	//}

	//[Test]
	//public void MoreControllerOpensOnFirstClick()
	//{
	//	App.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	App.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	App.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
	//	App.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	App.Tap(x => x.Class("UITabBarButton").Marked("Tab 4"));
	//	App.WaitForElement("Tab 4 Content");

	//	App.Tap(x => x.Class("UITabBarButton").Marked("More"));
	//	App.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 6"));
	//}

	//[Test]
	//public void MoreControllerDoesNotShowEditButton()
	//{
	//	App.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	App.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	App.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	Assert.AreEqual(App.Query(x => x.Marked("Edit")).Count(), 0);
	//}
}