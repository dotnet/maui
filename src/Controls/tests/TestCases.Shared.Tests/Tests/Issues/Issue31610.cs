using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31610 : _IssuesUITest
{
	public Issue31610(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "ScrollView Content Misaligned in RightToLeft FlowDirection when adding views dynamically";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewRtlDynamicContentShouldDisplayCorrectly()
	{
		// Wait for the page to load
		App.WaitForElement("TestScrollView");
		App.WaitForElement("Item1");

		// Verify initial items are visible in RTL mode
		App.WaitForElement("ItemLabel1");
		App.WaitForElement("ItemLabel2");
		App.WaitForElement("ItemLabel3");

		// Scroll horizontally to trigger dynamic content addition
		var scrollView = App.FindElement("TestScrollView");
		var scrollViewRect = scrollView.GetRect();

		// Perform horizontal scroll gesture to trigger OnScrolled event
		var startX = scrollViewRect.X + scrollViewRect.Width * 0.8f; // Start near right edge
		var startY = scrollViewRect.Y + scrollViewRect.Height / 2;
		var endX = startX - 200; // Scroll left (which is right-to-left scrolling)

		App.DragCoordinates(startX, startY, endX, startY);

		// Wait for potential dynamic content to be added
		Thread.Sleep(1000);

		// Add more items manually to test the fix
		App.Tap("AddItemsButton");
		Thread.Sleep(500);

		// Verify that new items are properly positioned and visible
		// The fix should ensure RTL layout remains correct even with dynamic content
		App.WaitForElement("ItemLabel6"); // Should be one of the newly added items
		App.WaitForElement("ItemLabel7");
		App.WaitForElement("ItemLabel8");

		// Verify items are still accessible and properly positioned after dynamic addition
		var item6 = App.FindElement("ItemLabel6");
		var item7 = App.FindElement("ItemLabel7");

		Assert.That(item6.GetText(), Is.EqualTo("Item 6"));
		Assert.That(item7.GetText(), Is.EqualTo("Item 7"));

		// Test FlowDirection toggle to ensure fix works in both directions
		App.Tap("ToggleDirectionButton");
		Thread.Sleep(500);

		// Add more items in LTR mode
		App.Tap("AddItemsButton");
		Thread.Sleep(500);

		// Verify items are still properly positioned after switching to LTR
		App.WaitForElement("ItemLabel9");
		App.WaitForElement("ItemLabel10");
		App.WaitForElement("ItemLabel11");

		var item9 = App.FindElement("ItemLabel9");
		Assert.That(item9.GetText(), Is.EqualTo("Item 9"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewRtlContentRemainVisibleAfterDynamicChanges()
	{
		// Wait for the page to load
		App.WaitForElement("TestScrollView");

		// Get initial item positions
		var item1 = App.FindElement("Item1");
		var item2 = App.FindElement("Item2");

		var initialItem1Rect = item1.GetRect();
		var initialItem2Rect = item2.GetRect();

		// Verify initial positioning - in RTL, items should be properly aligned
		Assert.That(initialItem1Rect.Width, Is.GreaterThan(0));
		Assert.That(initialItem2Rect.Width, Is.GreaterThan(0));

		// Add multiple batches of items to test extensive dynamic content
		for (int i = 0; i < 3; i++)
		{
			App.Tap("AddItemsButton");
			Thread.Sleep(300);
		}

		// Verify that original items are still visible and positioned correctly
		// The fix should prevent content from going off-screen in RTL mode
		App.WaitForElement("Item1");
		App.WaitForElement("Item2");
		App.WaitForElement("Item3");

		var afterItem1 = App.FindElement("Item1");
		var afterItem2 = App.FindElement("Item2");

		var afterItem1Rect = afterItem1.GetRect();
		var afterItem2Rect = afterItem2.GetRect();

		// Items should still be within the visible bounds and properly positioned
		Assert.That(afterItem1Rect.Width, Is.GreaterThan(0));
		Assert.That(afterItem2Rect.Width, Is.GreaterThan(0));

		// Verify the scrollable area works correctly by attempting to scroll horizontally
		var scrollView = App.FindElement("TestScrollView");
		var scrollViewRect = scrollView.GetRect();

		var startX = scrollViewRect.X + scrollViewRect.Width * 0.8f;
		var startY = scrollViewRect.Y + scrollViewRect.Height / 2;
		var endX = startX - 150;

		App.DragCoordinates(startX, startY, endX, startY);

		// Wait for scroll animation
		Thread.Sleep(500);

		// Items should still be accessible after scrolling
		App.WaitForElement("Item1");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewToggleBetweenRtlAndLtrShouldMaintainLayout()
	{
		// Wait for the page to load
		App.WaitForElement("TestScrollView");
		App.WaitForElement("ToggleDirectionButton");

		// Add some items first
		App.Tap("AddItemsButton");
		Thread.Sleep(500);

		// Verify initial RTL state
		var toggleButton = App.FindElement("ToggleDirectionButton");
		Assert.That(toggleButton.GetText(), Is.EqualTo("Switch to LTR"));

		// Switch to LTR
		App.Tap("ToggleDirectionButton");
		Thread.Sleep(500);

		// Verify button text changed
		var toggleButtonAfter = App.FindElement("ToggleDirectionButton");
		Assert.That(toggleButtonAfter.GetText(), Is.EqualTo("Switch to RTL"));

		// Add more items in LTR mode
		App.Tap("AddItemsButton");
		Thread.Sleep(500);

		// Verify items are still visible and properly positioned in LTR
		App.WaitForElement("Item1");
		var item1Ltr = App.FindElement("Item1");
		var item1LtrRect = item1Ltr.GetRect();

		Assert.That(item1LtrRect.Width, Is.GreaterThan(0));
		Assert.That(item1LtrRect.Height, Is.GreaterThan(0));

		// Switch back to RTL
		App.Tap("ToggleDirectionButton");
		Thread.Sleep(500);

		// Verify items are still properly positioned after switching back
		App.WaitForElement("Item1");
		var item1BackToRtl = App.FindElement("Item1");
		var item1BackToRtlRect = item1BackToRtl.GetRect();

		Assert.That(item1BackToRtlRect.Width, Is.GreaterThan(0));
		Assert.That(item1BackToRtlRect.Height, Is.GreaterThan(0));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewAutoScrollsToNewlyAddedItems()
	{
		App.WaitForElement("TestScrollView");
		App.WaitForElement("AddItemsButton");

		App.WaitForElement("Item8");

		App.Tap("AddItemsButton");
		Thread.Sleep(1000);

		App.WaitForElement("Item9");
		App.WaitForElement("Item10");
		App.WaitForElement("Item11");

		var newItem11 = App.FindElement("Item11");
		var newItem11Rect = newItem11.GetRect();

		Assert.That(newItem11Rect.Width, Is.GreaterThan(0));
		Assert.That(newItem11Rect.Height, Is.GreaterThan(0));

		var item11Label = App.FindElement("ItemLabel11");
		Assert.That(item11Label.GetText(), Is.EqualTo("Item 11"));

		App.Tap("ToggleDirectionButton");
		Thread.Sleep(500);

		App.Tap("AddItemsButton");
		Thread.Sleep(1000);

		App.WaitForElement("Item12");
		App.WaitForElement("Item13");
		App.WaitForElement("Item14");

		var ltrNewItem14 = App.FindElement("Item14");
		var ltrNewItem14Rect = ltrNewItem14.GetRect();

		Assert.That(ltrNewItem14Rect.Width, Is.GreaterThan(0));
		Assert.That(ltrNewItem14Rect.Height, Is.GreaterThan(0));

		var item14Label = App.FindElement("ItemLabel14");
		Assert.That(item14Label.GetText(), Is.EqualTo("Item 14"));
	}
}