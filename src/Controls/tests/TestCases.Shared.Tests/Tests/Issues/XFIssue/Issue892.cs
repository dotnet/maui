using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue892 : _IssuesUITest
{
	public Issue892(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NavigationPages as details in FlyoutPage don't work as expected";

	//[Test]
	//[Category(UITestCategories.FlyoutPage)]
	//[FailsOnAndroid]
	//[Description("Change pages in the Flyout ListView, and navigate to the end and back")]
	//public void Issue892TestsNavigateChangePagesNavigate()
	//{
	//	NavigateToEndAndBack();

	//	RunningApp.Tap(q => q.Marked("Present Flyout"));

	//	RunningApp.Tap(q => q.Marked("Page 5"));

	//	RunningApp.Tap(q => q.Marked("Close Flyout"));

	//	RunningApp.Screenshot("Select new detail navigation");

	//	NavigateToEndAndBack();
	//}

	//void NavigateToEndAndBack()
	//{
	//	RunningApp.WaitForElement(q => q.Button("Push next page")); // still required on iOS
	//	RunningApp.Tap(q => q.Marked("Push next page"));
	//	RunningApp.Screenshot("Pushed first page");

	//	RunningApp.WaitForElement(q => q.Button("Push next next page")); // still required on iOS
	//	RunningApp.Tap(q => q.Marked("Push next next page"));
	//	RunningApp.Screenshot("Pushed second page");

	//	RunningApp.WaitForElement(q => q.Marked("You are at the end of the line"));
	//	RunningApp.Screenshot("Pushed last page");

	//	RunningApp.Tap(q => q.Marked("Check back one"));
	//	RunningApp.WaitForElement(q => q.Marked("Pop one"));
	//	RunningApp.Back();
	//	RunningApp.Screenshot("Navigate Back");

	//	RunningApp.Tap(q => q.Marked("Check back two"));
	//	RunningApp.WaitForElement(q => q.Marked("Pop two"));
	//	RunningApp.Back();
	//	RunningApp.Screenshot("Navigate Back");

	//	RunningApp.Tap(q => q.Marked("Check back three"));
	//	RunningApp.WaitForElement(q => q.Marked("At root"));
	//	RunningApp.Screenshot("At root");
	//}
}