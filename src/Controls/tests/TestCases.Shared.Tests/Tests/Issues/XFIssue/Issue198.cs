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
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// 	public void Issue198TestsNREWithPopModal()
	// 	{
	// 		App.WaitForElement(q => q.Marked("Page One"));
	// 		App.WaitForElement(q => q.Button("Leave"));
	// 		App.Screenshot("All Elements Present");

	// 		App.Tap(q => q.Marked("Leave"));
	// 		App.Screenshot("Clicked Leave");

	// 		App.WaitForElement(q => q.Marked("Bug Repro's"));
	// #if !__MACOS__
	// 		App.ClearText(q => q.Raw("* marked:'SearchBarGo'"));
	// 		App.EnterText(q => q.Raw("* marked:'SearchBarGo'"), "G198");
	// #endif
	// 		App.Tap(q => q.Marked("SearchButton"));
	// 		App.Screenshot("Navigate into gallery again");

	// 		App.WaitForElement(q => q.Marked("Page Three"));
	// 		App.Tap(q => q.Marked("Page Three"));

	// 		App.WaitForElement(q => q.Marked("No Crash"));
	// 		App.Screenshot("App did not crash");
	// 	}
}
