using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2953 : _IssuesUITest
{
	public Issue2953(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "GroupHeaderCells disappear when item is removed from a group in ListView (iOS only) ";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Issue2953Test()
	// {
	// 	RunningApp.Screenshot("I am at Issue 2953");
	// 	RunningApp.WaitForElement(q => q.Marked("Header 3"));
	// 	RunningApp.Screenshot("I see the Header 3");
	// 	RunningApp.Tap(q => q.Marked("btnRemove"));
	// 	RunningApp.WaitForElement(q => q.Marked("Header 3"));
	// 	RunningApp.Screenshot("I still see the Header 3");
	// }
}