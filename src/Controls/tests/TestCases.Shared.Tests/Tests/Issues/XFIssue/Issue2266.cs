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
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void SwapMainPageWithFlyoutPages()
	// {
	// 	App.WaitForElement(q => q.Text("FlyoutPage Navigation"));
	// 	App.Tap(q => q.Text("FlyoutPage Navigation"));
	// 	App.Tap(q => q.Marked("OpenMaster"));
	// 	App.Tap(q => q.Text("Page 1"));
	// 	App.Tap(q => q.Text("START"));
	// 	App.Tap(q => q.Text("FlyoutPage Navigation ->> Page 1"));
	// 	App.WaitForElement(q => q.Text("Page 1"));
	// 	App.Tap(q => q.Text("START"));
	// 	App.Tap(q => q.Text("FlyoutPage Navigation ->> Page 2"));
	// 	App.WaitForElement(q => q.Text("Page 2"));
	// 	App.Tap(q => q.Text("START"));
	// 	App.Tap(q => q.Text("FlyoutPage Navigation ->> Page 3"));
	// 	App.WaitForElement(q => q.Text("Page 3"));
	// }
}
