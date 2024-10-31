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
	// 	App.WaitForElement(q => q.Marked("FirstPageLabel"));
	// 	App.Screenshot("First Tab is showing");
	// }

	// [Test]
	// public void Issue2923TestTwo()
	// {
	// 	App.Tap(q => q.Marked("ResetButton"));
	// 	App.Screenshot("Tabs Reset");
	// 	App.WaitForElement(q => q.Marked("ResetPageLabel"));
	// }
}