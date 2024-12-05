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
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void CanScrollToGroupAndItemIndex()
	//{
	//	App.WaitForElement("GroupIndexEntry");
	//	App.Tap("GroupIndexEntry");
	//	App.ClearText();
	//	App.EnterText("5");

	//	App.Tap("ItemIndexEntry");
	//	App.ClearText();
	//	App.EnterText("1");

	//	App.Tap("GoButton");

	//	// Should scroll enough to display this item
	//	App.WaitForElement("Squirrel Girl");
	//}

	//[Test]
	//public void InvalidScrollToIndexShouldNotCrash()
	//{
	//	App.WaitForElement("GroupIndexEntry");
	//	App.Tap("GroupIndexEntry");
	//	App.ClearText();
	//	App.EnterText("55");

	//	App.Tap("ItemIndexEntry");
	//	App.ClearText();
	//	App.EnterText("1");

	//	App.Tap("GoButton");

	//	// Should scroll enough to display this item
	//	App.WaitForElement("Avengers");
	//}

	//[Test]
	//[Compatibility.UITests.FailsOnIOSWhenRunningOnXamarinUITest]
	//public void CanScrollToGroupAndItem()
	//{
	//	App.WaitForElement("GroupNameEntry");
	//	App.Tap("GroupNameEntry");
	//	App.ClearText();
	//	App.EnterText("Heroes for Hire");

	//	App.Tap("ItemNameEntry");
	//	App.ClearText();
	//	App.EnterText("Misty Knight");

	//	App.Tap("GoItemButton");

	//	// Should scroll enough to display this item
	//	App.WaitForElement("Luke Cage");
	//}
}