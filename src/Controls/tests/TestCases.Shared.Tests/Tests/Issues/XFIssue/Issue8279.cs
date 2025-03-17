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
	const string ScrollWithNoItemButGroup = "ScrollWithNoItemButGroup";
	const string ScrollWithItemButNoGroup = "ScrollWithItemButNoGroup";
	const string ScrollWithItemWithGroup = "ScrollWithItemWithGroup";
	const string ScrollWithNoItemNoGroup = "ScrollWithNoItemNoGroup";
	const string ScrollWithNoItemEmptyGroup = "ScrollWithNoItemEmptyGroup";
	const string ResetButton = "Reset";
	public override string Issue => "[Feature requested] ListView do not ScrollTo a group when there is no child of this group";

	[Test]
	public void AScrollWithNoItemButGroupTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ScrollWithNoItemButGroup);
		App.Tap(ScrollWithNoItemButGroup);
		// This will fail if the list didn't scroll. If it did scroll, it will succeed
		App.WaitForElement("Header 3");
	}

	[Test]
	public void BScrollWithItemButNoGroupTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ScrollWithItemButNoGroup);
		App.Tap(ScrollWithItemButNoGroup);
		// This will fail if the list didn't scroll. If it did scroll, it will succeed
		App.WaitForElement("title 1");
	}

	[Test]
	public void CScrollWithItemWithGroupTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ScrollWithItemWithGroup);
		App.Tap(ScrollWithItemWithGroup);
		// This will fail if the list didn't scroll. If it did scroll, it will succeed
		App.WaitForElement("Header 3");
	}

	[Test]
	public void DScrollWithNoItemNoGroupTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ScrollWithNoItemNoGroup);
		App.Tap(ScrollWithNoItemNoGroup);
		// This will pass if the list didn't scroll and remain on the same state
		App.WaitForElement("Header 1");
	}

	[Test]
	public void EScrollWithNoItemEmptyGroupTest()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(ScrollWithNoItemEmptyGroup);
		App.Tap(ScrollWithNoItemEmptyGroup);
		// This will fail if the list didn't scroll. If it did scroll, it will succeed
		App.WaitForElement("Header 2");
	}
}