#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30950 : _IssuesUITest
{
    public Issue30950(TestDevice device)
        : base(device)
    {
    }

    public override string Issue => "Deleting Items Causes Other Expanded Items to Collapse Unexpectedly";

    [Test]
    [Category(UITestCategories.SwipeView)]
    [Category(UITestCategories.CollectionView)]
    public void SwipeViewItemsShouldRemainExpandedWhenOtherItemsAreDeleted()
    {
        const string itemToDelete = "Item 6";
        var itemsToSwipe = new[] { 2, 3, 4, 5, 7 };

        TestContext.WriteLine("Starting SwipeView collapse test");

        // Expand multiple items
        foreach (var itemNumber in itemsToSwipe)
        {
            var itemId = $"Item {itemNumber}";
            App.WaitForElement(itemId);
            App.SwipeLeftToRight(itemId);
            TestContext.WriteLine($"Swiped {itemId}");
        }

        // Confirm swipe actions are visible
        App.WaitForElement("Favourite");
        App.WaitForElement("Delete");
        TestContext.WriteLine("Confirmed swipe actions are visible");

        // Delete target item
        App.WaitForElement(itemToDelete);
        App.SwipeLeftToRight(itemToDelete);
        TestContext.WriteLine($"Swiped {itemToDelete}");

        var itemElement = App.WaitForElement(itemToDelete);
        var itemRect = itemElement.GetRect();
        var deleteButtonX = itemRect.X + 100;
        var deleteButtonY = itemRect.Y + itemRect.Height / 2;
        App.TapCoordinates(deleteButtonX, deleteButtonY);
        TestContext.WriteLine($"Tapped delete button for {itemToDelete}");

        Thread.Sleep(500);

        // Verify other items remain expanded
        try
        {
            App.WaitForElement("Favourite", timeout: TimeSpan.FromSeconds(2));
            App.WaitForElement("Delete", timeout: TimeSpan.FromSeconds(2));
            TestContext.WriteLine("Verified that other items remain expanded after deletion");
        }
        catch
        {
            Assert.Fail("SwipeView items collapsed unexpectedly after deletion. Regression suspected.");
        }
        finally
        {
            // Close opened items
            foreach (var itemNumber in itemsToSwipe)
            {
                var itemId = $"Item {itemNumber}";
                App.WaitForElement(itemId);
                App.Tap(itemId);
            }
        }
    }
}
#endif