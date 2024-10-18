using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.CollectionView)]
public class ScrollToGroup : _IssuesUITest
{
	public ScrollToGroup(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView Scroll To Grouped Item";

	// TODO: HostApp UI pushes some ControlGallery specific page? Commented out now, fix that first!
	//[Test]
	//[FailsOnIOS]
	//public void CanScrollToGroupAndItemIndex()
	//{
	//	RunningApp.WaitForElement("GroupIndexEntry");
	//	RunningApp.Tap("GroupIndexEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("5");

	//	RunningApp.Tap("ItemIndexEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("1");

	//	RunningApp.Tap("GoButton");

	//	// Should scroll enough to display this item
	//	RunningApp.WaitForElement("Squirrel Girl");
	//}

	//[Test]
	//public void InvalidScrollToIndexShouldNotCrash()
	//{
	//	RunningApp.WaitForElement("GroupIndexEntry");
	//	RunningApp.Tap("GroupIndexEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("55");

	//	RunningApp.Tap("ItemIndexEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("1");

	//	RunningApp.Tap("GoButton");

	//	// Should scroll enough to display this item
	//	RunningApp.WaitForElement("Avengers");
	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOS]
	//public void CanScrollToGroupAndItem()
	//{
	//	RunningApp.WaitForElement("GroupNameEntry");
	//	RunningApp.Tap("GroupNameEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("Heroes for Hire");

	//	RunningApp.Tap("ItemNameEntry");
	//	RunningApp.ClearText();
	//	RunningApp.EnterText("Misty Knight");

	//	RunningApp.Tap("GoItemButton");

	//	// Should scroll enough to display this item
	//	RunningApp.WaitForElement("Luke Cage");
	//}
}