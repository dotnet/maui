using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2266 : _IssuesUITest
{
	public Issue2266(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting a different Detail page from a FlyoutPage after 2nd time on MainPage";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// [FailsOnIOS]
	// public void SwapMainPageWithFlyoutPages()
	// {
	// 	RunningApp.WaitForElement(q => q.Text("FlyoutPage Navigation"));
	// 	RunningApp.Tap(q => q.Text("FlyoutPage Navigation"));
	// 	RunningApp.Tap(q => q.Marked("OpenMaster"));
	// 	RunningApp.Tap(q => q.Text("Page 1"));
	// 	RunningApp.Tap(q => q.Text("START"));
	// 	RunningApp.Tap(q => q.Text("FlyoutPage Navigation ->> Page 1"));
	// 	RunningApp.WaitForElement(q => q.Text("Page 1"));
	// 	RunningApp.Tap(q => q.Text("START"));
	// 	RunningApp.Tap(q => q.Text("FlyoutPage Navigation ->> Page 2"));
	// 	RunningApp.WaitForElement(q => q.Text("Page 2"));
	// 	RunningApp.Tap(q => q.Text("START"));
	// 	RunningApp.Tap(q => q.Text("FlyoutPage Navigation ->> Page 3"));
	// 	RunningApp.WaitForElement(q => q.Text("Page 3"));
	// }
}
