using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue264 : _IssuesUITest
{
	public Issue264(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PopModal NRE";

	// [Test]
	// [Category(UITestCategories.DisplayAlert)]
	// [FailsOnIOS]
	// public void Issue264TestsPushAndPopModal()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked("Home"));
	// 	RunningApp.WaitForElement(q => q.Button("About"));
	// 	RunningApp.Screenshot("All elements present");

	// 	RunningApp.Tap(q => q.Button("About"));
	// 	RunningApp.WaitForElement(q => q.Button("Close"));
	// 	RunningApp.Screenshot("Modal pushed");

	// 	RunningApp.Tap(q => q.Button("Close"));
	// 	RunningApp.WaitForElement(q => q.Button("About"));
	// 	RunningApp.Screenshot("Modal popped");

	// 	RunningApp.Tap(q => q.Button("Pop me"));
	// 	RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));
	// 	RunningApp.Screenshot("No crash");
	// }
}
