using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16020 : _IssuesUITest
{
	public Issue16020(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Newly created recipes cannot be deleted";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void CarouselViewiOSCrashPreventionTest()
    {
        // Wait for the page to load completely
        App.WaitForElement("My Recipes");
        App.Tap("My Recipes");
        App.WaitForElement("MyCarouselView");
        
        // Check initial count should be 5 items
        App.WaitForElement("CarouselViewCountLabel");
        var initialCountText = App.WaitForElement("CarouselViewCountLabel").GetText();
        Assert.That(initialCountText, Is.EqualTo("Items Count: 5"), "Initial count should be 5 items");
        
        App.Tap("GoToLastIndexButton");
        App.WaitForElement("Beef Tacos");
        App.Tap("Beef Tacos");
        App.WaitForElement("Recipe Name:");
        App.Tap("Edit Recipe");
        App.WaitForElement("Delete Recipe");
        App.Tap("DeleteRecipeButton");
        
        // Verify count is updated after deletion - should be 4 items now
        App.WaitForElement("CarouselViewCountLabel");
        var updatedCountText = App.WaitForElement("CarouselViewCountLabel").GetText();
        Assert.That(updatedCountText, Is.EqualTo("Items Count: 4"), "Count should be 4 items after deletion");
    }
}
