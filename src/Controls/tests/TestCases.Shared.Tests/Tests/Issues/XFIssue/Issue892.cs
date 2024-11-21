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
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//[Description("Change pages in the Flyout ListView, and navigate to the end and back")]
	//public void Issue892TestsNavigateChangePagesNavigate()
	//{
	//	NavigateToEndAndBack();

	//	App.Tap(q => q.Marked("Present Flyout"));

	//	App.Tap(q => q.Marked("Page 5"));

	//	App.Tap(q => q.Marked("Close Flyout"));

	//	App.Screenshot("Select new detail navigation");

	//	NavigateToEndAndBack();
	//}

	//void NavigateToEndAndBack()
	//{
	//	App.WaitForElement(q => q.Button("Push next page")); // still required on iOS
	//	App.Tap(q => q.Marked("Push next page"));
	//	App.Screenshot("Pushed first page");

	//	App.WaitForElement(q => q.Button("Push next next page")); // still required on iOS
	//	App.Tap(q => q.Marked("Push next next page"));
	//	App.Screenshot("Pushed second page");

	//	App.WaitForElement(q => q.Marked("You are at the end of the line"));
	//	App.Screenshot("Pushed last page");

	//	App.Tap(q => q.Marked("Check back one"));
	//	App.WaitForElement(q => q.Marked("Pop one"));
	//	App.Back();
	//	App.Screenshot("Navigate Back");

	//	App.Tap(q => q.Marked("Check back two"));
	//	App.WaitForElement(q => q.Marked("Pop two"));
	//	App.Back();
	//	App.Screenshot("Navigate Back");

	//	App.Tap(q => q.Marked("Check back three"));
	//	App.WaitForElement(q => q.Marked("At root"));
	//	App.Screenshot("At root");
	//}
}