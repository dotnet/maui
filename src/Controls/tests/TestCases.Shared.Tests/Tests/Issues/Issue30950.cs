#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30950 : _IssuesUITest
{
    const int NumberOfIterations = 2;
    
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
        // Define which items to delete in each iteration
        var itemsToDelete = new[] { "Item 1", "Item 6" }; // Different item for each iteration
        
        // Repeat the test scenario 2 times to ensure consistency
        for (int iteration = 1; iteration <= NumberOfIterations; iteration++)
        {
            TestContext.WriteLine($"Starting iteration {iteration}");
            
            try
            {
                var itemToDelete = itemsToDelete[iteration - 1];
                PerformSwipeAndDeleteTest(iteration, itemToDelete);
                TestContext.WriteLine($"Iteration {iteration} completed successfully");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test failed on iteration {iteration}: {ex.Message}");
            }
            
            // Add a small delay between iterations to allow UI to settle
            if (iteration < NumberOfIterations)
            {
                System.Threading.Thread.Sleep(1000);
                TestContext.WriteLine($"Completed iteration {iteration}, preparing for next iteration");
            }
        }
    }
    
    void PerformSwipeAndDeleteTest(int iteration, string itemToDelete)
    {
        // Swipe multiple items to expand them (excluding the item we plan to delete)
        var itemsToSwipe = new[] { 2, 3, 4, 5, 7 };
        foreach (var itemNumber in itemsToSwipe)
        {
            var itemId = $"Item {itemNumber}";
            
            // Skip if this is the item we plan to delete (to test deleting non-expanded items)
            if (itemId == itemToDelete)
                continue;
            
            try
            {
                App.WaitForElement(itemId);
                App.SwipeLeftToRight(itemId);
                TestContext.WriteLine($"Iteration {iteration}: Swiped {itemId}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Iteration {iteration}: Failed to swipe {itemId}: {ex.Message}");
                // Continue with other items or re-throw based on your needs
                throw;
            }
        }
        
        // Verify swipe actions are visible
        App.WaitForElement("Favourite");
        App.WaitForElement("Delete");
        TestContext.WriteLine($"Iteration {iteration}: Confirmed swipe actions are visible");

        // Delete the specified item by swiping and tapping delete
        try
        {
            App.WaitForElement(itemToDelete);
            App.SwipeLeftToRight(itemToDelete);
            TestContext.WriteLine($"Iteration {iteration}: Swiped {itemToDelete}");
        
            var itemElement = App.WaitForElement(itemToDelete);
            var itemRect = itemElement.GetRect();
               
            // Tap in the area where the delete button should be
            var deleteButtonX = itemRect.X + 100;
            var deleteButtonY = itemRect.Y + itemRect.Height / 2;
            App.TapCoordinates(deleteButtonX, deleteButtonY);
            TestContext.WriteLine($"Iteration {iteration}: Tapped delete button for {itemToDelete}");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Iteration {iteration}: Failed to delete {itemToDelete}: {ex.Message}");
            throw;
        }
        
        // Small delay to allow deletion to process
        Thread.Sleep(500);
       
        // Verify that items are still showing their swipe actions
        try
        {
	        App.WaitForElement("Favourite", timeout: TimeSpan.FromSeconds(3));
	        App.WaitForElement("Delete", timeout: TimeSpan.FromSeconds(3));
	        TestContext.WriteLine(
		        $"Iteration {iteration}: Verified that other items remain expanded after deleting {itemToDelete}");
        }
        catch
        {
	        Assert.Fail(
		        $"Iteration {iteration}: SwipeView items collapsed unexpectedly when {itemToDelete} was deleted. The fix for Issue79I10Swipe is not working.");
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