using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue198 : _IssuesUITest
{
	public Issue198(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage shouldn't proxy content of NavigationPage";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// [FailsOnIOS]
	// 	public void Issue198TestsNREWithPopModal()
	// 	{
	// 		RunningApp.WaitForElement(q => q.Marked("Page One"));
	// 		RunningApp.WaitForElement(q => q.Button("Leave"));
	// 		RunningApp.Screenshot("All Elements Present");

	// 		RunningApp.Tap(q => q.Marked("Leave"));
	// 		RunningApp.Screenshot("Clicked Leave");

	// 		RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));
	// #if !__MACOS__
	// 		RunningApp.ClearText(q => q.Raw("* marked:'SearchBarGo'"));
	// 		RunningApp.EnterText(q => q.Raw("* marked:'SearchBarGo'"), "G198");
	// #endif
	// 		RunningApp.Tap(q => q.Marked("SearchButton"));
	// 		RunningApp.Screenshot("Navigate into gallery again");

	// 		RunningApp.WaitForElement(q => q.Marked("Page Three"));
	// 		RunningApp.Tap(q => q.Marked("Page Three"));

	// 		RunningApp.WaitForElement(q => q.Marked("No Crash"));
	// 		RunningApp.Screenshot("App did not crash");
	// 	}
}
