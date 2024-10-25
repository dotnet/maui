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
	//	RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
	//	RunningApp.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	RunningApp.WaitForElement(x => x.Text("Success"));
	//}

	//[Test]
	//public void MoreControllerOpensOnFirstClick()
	//{
	//	RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
	//	RunningApp.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	RunningApp.Tap(x => x.Class("UITabBarButton").Marked("Tab 4"));
	//	RunningApp.WaitForElement("Tab 4 Content");

	//	RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));
	//	RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 6"));
	//}

	//[Test]
	//public void MoreControllerDoesNotShowEditButton()
	//{
	//	RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
	//	RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

	//	RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));

	//	Assert.AreEqual(RunningApp.Query(x => x.Marked("Edit")).Count(), 0);
	//}
}