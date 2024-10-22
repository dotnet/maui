using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class Issue2923 : _IssuesUITest
{
	public Issue2923(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "First tab does not load until navigating";

	// [Test]
	// public void Issue2923TestOne()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked("FirstPageLabel"));
	// 	RunningApp.Screenshot("First Tab is showing");
	// }

	// [Test]
	// public void Issue2923TestTwo()
	// {
	// 	RunningApp.Tap(q => q.Marked("ResetButton"));
	// 	RunningApp.Screenshot("Tabs Reset");
	// 	RunningApp.WaitForElement(q => q.Marked("ResetPageLabel"));
	// }
}