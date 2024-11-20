#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32801 : _IssuesUITest
{
	public Bugzilla32801(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Memory Leak in TabbedPage + NavigationPage";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void Bugzilla32801Test()
	// {
	// 	App.Tap("btnAdd");
	// 	App.Tap("btnAdd");
	// 	App.Tap("btnStack");
	// 	App.WaitForElement("Stack 3");
	// 	App.Tap("Tab");
	// 	App.Tap("btnStack");
	// 	App.WaitForElement("Stack 1");
	// }

	// [TearDown]
	// public void TearDown()
	// {
	// 	App.SetOrientationPortrait();
	// }
}
#endif