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
	// 	App.Screenshot("I am at Issue 2953");
	// 	App.WaitForElement(q => q.Marked("Header 3"));
	// 	App.Screenshot("I see the Header 3");
	// 	App.Tap(q => q.Marked("btnRemove"));
	// 	App.WaitForElement(q => q.Marked("Header 3"));
	// 	App.Screenshot("I still see the Header 3");
	// }
}