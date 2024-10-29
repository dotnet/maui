using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ListView)]
public class Issue8279 : _IssuesUITest
{
	public Issue8279(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Feature requested] ListView do not ScrollTo a group when there is no child of this group";

	//[Test]
	//public void ScrollWithNoItemButGroupTest()
	//{
	//	App.WaitForElement(Reset);
	//	App.Tap(Reset);
	//	App.WaitForElement(ScrollWithNoItemButGroup);
	//	App.Tap(ScrollWithNoItemButGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	App.WaitForElement(q => q.Marked("Header 3"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithItemButNoGroupTest()
	//{
	//	App.WaitForElement(Reset);
	//	App.Tap(Reset);
	//	App.WaitForElement(ScrollWithItemButNoGroup);
	//	App.Tap(ScrollWithItemButNoGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	App.WaitForElement(q => q.Marked("title 1"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithItemWithGroupTest()
	//{
	//	App.WaitForElement(Reset);
	//	App.Tap(Reset);
	//	App.WaitForElement(ScrollWithItemWithGroup);
	//	App.Tap(ScrollWithItemWithGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	App.WaitForElement(q => q.Marked("Header 3"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithNoItemNoGroupTest()
	//{
	//	App.WaitForElement(Reset);
	//	App.Tap(Reset);
	//	App.WaitForElement(ScrollWithNoItemNoGroup);
	//	App.Tap(ScrollWithNoItemNoGroup);
	//	// This will pass if the list didn't scroll and remain on the same state
	//	App.WaitForElement(q => q.Marked("Header 1"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithNoItemEmptyGroupTest()
	//{
	//	App.WaitForElement(Reset);
	//	App.Tap(Reset);
	//	App.WaitForElement(ScrollWithNoItemEmptyGroup);
	//	App.Tap(ScrollWithNoItemEmptyGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	App.WaitForElement(q => q.Marked("Header 2"), timeout: TimeSpan.FromSeconds(2));
	//}
}