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
	// 	App.WaitForElement(q => q.Marked("Home"));
	// 	App.WaitForElement(q => q.Button("About"));
	// 	App.Screenshot("All elements present");

	// 	App.Tap(q => q.Button("About"));
	// 	App.WaitForElement(q => q.Button("Close"));
	// 	App.Screenshot("Modal pushed");

	// 	App.Tap(q => q.Button("Close"));
	// 	App.WaitForElement(q => q.Button("About"));
	// 	App.Screenshot("Modal popped");

	// 	App.Tap(q => q.Button("Pop me"));
	// 	App.WaitForElement(q => q.Marked("Bug Repro's"));
	// 	App.Screenshot("No crash");
	// }
}
