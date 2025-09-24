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
		App.WaitForElement("TestScrollView");
		App.WaitForElement("Item1");

		App.WaitForElement("ItemLabel1");
		App.WaitForElement("ItemLabel2");
		App.WaitForElement("ItemLabel3");

		var scrollView = App.FindElement("TestScrollView");
		var scrollViewRect = scrollView.GetRect();

		var startX = scrollViewRect.X + scrollViewRect.Width * 0.8f;
		var startY = scrollViewRect.Y + scrollViewRect.Height / 2;
		var endX = startX - 200;

		App.DragCoordinates(startX, startY, endX, startY);

		App.Tap("AddItemsButton");

		App.WaitForElement("ItemLabel6");
		App.WaitForElement("ItemLabel7");
		App.WaitForElement("ItemLabel8");

		var item6 = App.FindElement("ItemLabel6");
		var item7 = App.FindElement("ItemLabel7");

		Assert.That(item6.GetText(), Is.EqualTo("Item 6"));
		Assert.That(item7.GetText(), Is.EqualTo("Item 7"));

		App.Tap("ToggleDirectionButton");

		App.Tap("AddItemsButton");

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
		App.WaitForElement("TestScrollView");

		var item1 = App.FindElement("Item1");
		var item2 = App.FindElement("Item2");

		var initialItem1Rect = item1.GetRect();
		var initialItem2Rect = item2.GetRect();

		Assert.That(initialItem1Rect.Width, Is.GreaterThan(0));
		Assert.That(initialItem2Rect.Width, Is.GreaterThan(0));

		for (int i = 0; i < 3; i++)
		{
			App.Tap("AddItemsButton");
		}

		App.WaitForElement("Item1");
		App.WaitForElement("Item2");
		App.WaitForElement("Item3");

		var afterItem1 = App.FindElement("Item1");
		var afterItem2 = App.FindElement("Item2");

		var afterItem1Rect = afterItem1.GetRect();
		var afterItem2Rect = afterItem2.GetRect();

		Assert.That(afterItem1Rect.Width, Is.GreaterThan(0));
		Assert.That(afterItem2Rect.Width, Is.GreaterThan(0));

		var scrollView = App.FindElement("TestScrollView");
		var scrollViewRect = scrollView.GetRect();

		var startX = scrollViewRect.X + scrollViewRect.Width * 0.8f;
		var startY = scrollViewRect.Y + scrollViewRect.Height / 2;
		var endX = startX - 150;

		App.DragCoordinates(startX, startY, endX, startY);

		App.WaitForElement("Item1");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewToggleBetweenRtlAndLtrShouldMaintainLayout()
	{
		App.WaitForElement("TestScrollView");
		App.WaitForElement("ToggleDirectionButton");

		App.Tap("AddItemsButton");

		var toggleButton = App.FindElement("ToggleDirectionButton");
		Assert.That(toggleButton.GetText(), Is.EqualTo("Switch to LTR"));

		App.Tap("ToggleDirectionButton");

		var toggleButtonAfter = App.FindElement("ToggleDirectionButton");
		Assert.That(toggleButtonAfter.GetText(), Is.EqualTo("Switch to RTL"));

		App.Tap("AddItemsButton");

		App.WaitForElement("Item1");
		var item1Ltr = App.FindElement("Item1");
		var item1LtrRect = item1Ltr.GetRect();

		Assert.That(item1LtrRect.Width, Is.GreaterThan(0));
		Assert.That(item1LtrRect.Height, Is.GreaterThan(0));

		App.Tap("ToggleDirectionButton");

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

		App.Tap("AddItemsButton");

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