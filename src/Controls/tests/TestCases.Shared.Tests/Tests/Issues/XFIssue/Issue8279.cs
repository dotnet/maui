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
	//	RunningApp.WaitForElement(Reset);
	//	RunningApp.Tap(Reset);
	//	RunningApp.WaitForElement(ScrollWithNoItemButGroup);
	//	RunningApp.Tap(ScrollWithNoItemButGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	RunningApp.WaitForElement(q => q.Marked("Header 3"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithItemButNoGroupTest()
	//{
	//	RunningApp.WaitForElement(Reset);
	//	RunningApp.Tap(Reset);
	//	RunningApp.WaitForElement(ScrollWithItemButNoGroup);
	//	RunningApp.Tap(ScrollWithItemButNoGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	RunningApp.WaitForElement(q => q.Marked("title 1"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithItemWithGroupTest()
	//{
	//	RunningApp.WaitForElement(Reset);
	//	RunningApp.Tap(Reset);
	//	RunningApp.WaitForElement(ScrollWithItemWithGroup);
	//	RunningApp.Tap(ScrollWithItemWithGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	RunningApp.WaitForElement(q => q.Marked("Header 3"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithNoItemNoGroupTest()
	//{
	//	RunningApp.WaitForElement(Reset);
	//	RunningApp.Tap(Reset);
	//	RunningApp.WaitForElement(ScrollWithNoItemNoGroup);
	//	RunningApp.Tap(ScrollWithNoItemNoGroup);
	//	// This will pass if the list didn't scroll and remain on the same state
	//	RunningApp.WaitForElement(q => q.Marked("Header 1"), timeout: TimeSpan.FromSeconds(2));
	//}

	//[Test]
	//public void ScrollWithNoItemEmptyGroupTest()
	//{
	//	RunningApp.WaitForElement(Reset);
	//	RunningApp.Tap(Reset);
	//	RunningApp.WaitForElement(ScrollWithNoItemEmptyGroup);
	//	RunningApp.Tap(ScrollWithNoItemEmptyGroup);
	//	// This will fail if the list didn't scroll. If it did scroll, it will succeed
	//	RunningApp.WaitForElement(q => q.Marked("Header 2"), timeout: TimeSpan.FromSeconds(2));
	//}
}