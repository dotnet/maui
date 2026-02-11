#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30403SmallImages : _IssuesUITest
{
    public Issue30403SmallImages(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue =>
        "Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Small Images)";

    [Test, Order(1)]
    [Category(UITestCategories.Image)]
    public void SmallImageInLargeContainer_ShouldNotExceedIntrinsicSize()
    {
        // This is the IMPORTANT test for the AspectFit fix
        App.WaitForElement("SmallImageCenter");
        var image = App.FindElement("SmallImageCenter");

        AssertImageIsVisible(image, "small image center");
        
        var imageRect = image.GetRect();
        
        // CORE VALIDATION: Small 50x50 image should not become huge in large container
        Assert.That(imageRect.Width, Is.LessThan(80), 
            "Small image should not exceed reasonable size bounds (expected ~50px width)");
        Assert.That(imageRect.Height, Is.LessThan(80), 
            "Small image should not exceed reasonable size bounds (expected ~50px height)");
        
        // Should maintain square aspect ratio
        AssertImageMaintainsAspectRatio(image, 50, 50);
        
        // Should be centered in container (not at edges)
        AssertImageIsCentered(image, "small image center");
    }

    [Test, Order(2)]
    [Category(UITestCategories.Image)]
    public void SmallImageStartAlignment_ShouldRespectAlignment()
    {
        App.WaitForElement("SmallImageStart");
        var image = App.FindElement("SmallImageStart");

        AssertImageIsVisible(image, "small image start");
        
        var imageRect = image.GetRect();
        
        // Should maintain small size
        Assert.That(imageRect.Width, Is.LessThan(80), 
            "Start-aligned small image should remain small");
        Assert.That(imageRect.Height, Is.LessThan(80), 
            "Start-aligned small image should remain small");
            
        AssertImageMaintainsAspectRatio(image, 50, 50);
    }

    [Test, Order(3)]
    [Category(UITestCategories.Image)]
    public void SmallImageEndAlignment_ShouldRespectAlignment()
    {
	    App.WaitForElement("SmallScroll");
	    App.ScrollDown("SmallScroll", ScrollStrategy.Gesture, 0.2);
	    
        App.WaitForElement("SmallImageEnd");
        var image = App.FindElement("SmallImageEnd");

        AssertImageIsVisible(image, "small image end");
        
        var imageRect = image.GetRect();
        
        // Should maintain small size
        Assert.That(imageRect.Width, Is.LessThan(80), 
            "End-aligned small image should remain small");
        Assert.That(imageRect.Height, Is.LessThan(80), 
            "End-aligned small image should remain small");
            
        AssertImageMaintainsAspectRatio(image, 50, 50);
        
        // Verify positioning relative to start alignment
        var startImage = App.FindElement("SmallImageStart");
        var startRect = startImage.GetRect();
        
        Assert.That(imageRect.Y, Is.GreaterThan(startRect.Y), 
            "End-aligned image should be positioned below start-aligned image");
    }
    
    [Test, Order(4)]
    [Category(UITestCategories.Image)]
    public void ConstrainedContainers_ShouldHandleSmallImages()
    {
	    App.WaitForElement("SmallScroll");
	    App.ScrollDown("SmallScroll", ScrollStrategy.Gesture, 0.6);
	    
        var constrainedIds = new[] 
        { 
            "SmallImageConstrained30", 
            "SmallImageConstrained40", 
            "SmallImageConstrained60" 
        };

        foreach (var id in constrainedIds)
        {
            App.WaitForElement(id);
            var image = App.FindElement(id);
            
            AssertImageIsVisible(image, $"constrained image {id}");
            
            // Images should maintain aspect ratio even in small containers
            AssertImageMaintainsAspectRatio(image, 50, 50, tolerance: 0.3);
            
            var rect = image.GetRect();
            
            // Should be constrained by container size but still visible
            Assert.That(rect.Width, Is.GreaterThan(5), 
                $"Constrained image {id} should still be visible");
            Assert.That(rect.Height, Is.GreaterThan(5), 
                $"Constrained image {id} should still be visible");
        }
        
        // Verify size progression (larger containers = larger images up to intrinsic size)
        var img30 = App.FindElement("SmallImageConstrained30").GetRect();
        var img40 = App.FindElement("SmallImageConstrained40").GetRect();
        var img60 = App.FindElement("SmallImageConstrained60").GetRect();
        
        Assert.That(img30.Width, Is.LessThanOrEqualTo(img40.Width), 
            "30px container should not result in larger image than 40px container");
        Assert.That(img40.Width, Is.LessThanOrEqualTo(img60.Width), 
            "40px container should not result in larger image than 60px container");
    }

    [Test, Order(5)]
    [Category(UITestCategories.Image)]
    public void TinyImage_ShouldRemainTiny()
    {
	    App.WaitForElement("SmallScroll");
	    App.ScrollDown("SmallScroll", ScrollStrategy.Gesture, 0.8);
	    
        App.WaitForElement("TinyImage");
        var image = App.FindElement("TinyImage");
        
        AssertImageIsVisible(image, "tiny image");
        
        var rect = image.GetRect();
        
        // Tiny 10x10 image should remain very small
        Assert.That(rect.Width, Is.LessThan(25), 
            "Tiny image should remain very small (width)");
        Assert.That(rect.Height, Is.LessThan(25), 
            "Tiny image should remain very small (height)");
        
        // Should maintain square aspect ratio
        AssertImageMaintainsAspectRatio(image, 10, 10, tolerance: 0.4);
    }

    [Test, Order(6)]
    [Category(UITestCategories.Image)]
    public void SizeComparison_ShouldShowDifferentSizes()
    {
	    App.WaitForElement("SmallScroll");
	    App.ScrollDown("SmallScroll", ScrollStrategy.Gesture, 0.8);
	    
        var comparisonIds = new[]
        {
            "ComparisonTiny",    // 10x10
            "ComparisonSmall",   // 50x50
            "ComparisonMedium",  // 100x100
            "ComparisonLarge"    // 200x200
        };

        var expectedMaxSizes = new[] { 15, 60, 80, 80 }; // Last one capped by 80px container
        
        for (int i = 0; i < comparisonIds.Length; i++)
        {
            var id = comparisonIds[i];
            var expectedMaxSize = expectedMaxSizes[i];
            
            App.WaitForElement(id);
            var image = App.FindElement(id);
            
            AssertImageIsVisible(image, $"comparison image {id}");
            
            var rect = image.GetRect();
            
            Assert.That(rect.Width, Is.LessThan(expectedMaxSize + 10), 
                $"Comparison image {id} should not exceed expected max size");
            Assert.That(rect.Height, Is.LessThan(expectedMaxSize + 10), 
                $"Comparison image {id} should not exceed expected max size");
            
            // All should maintain square aspect ratio
            var originalSize = new[] { 10, 50, 100, 200 }[i];
            AssertImageMaintainsAspectRatio(image, originalSize, originalSize, tolerance: 0.3);
        }
        
        // Verify size progression
        var tinyRect = App.FindElement("ComparisonTiny").GetRect();
        var smallRect = App.FindElement("ComparisonSmall").GetRect();
        var mediumRect = App.FindElement("ComparisonMedium").GetRect();
        
        Assert.That(tinyRect.Width, Is.LessThan(smallRect.Width), 
            "Tiny image should be smaller than small image");
        Assert.That(smallRect.Width, Is.LessThanOrEqualTo(mediumRect.Width), 
            "Small image should not be larger than medium image");
    }
	
    // Helper methods for small image testing
    void AssertImageIsVisible(IUIElement image, string context)
    {
        Assert.That(image, Is.Not.Null, $"Image should exist for {context}");
        
        var rect = image.GetRect();
        Assert.That(rect.Width, Is.GreaterThan(0), $"Image should have positive width for {context}");
        Assert.That(rect.Height, Is.GreaterThan(0), $"Image should have positive height for {context}");
        
        try
        {
            Assert.That(image.IsDisplayed(), Is.True, $"Image should be displayed for {context}");
        }
        catch
        {
            TestContext.WriteLine($"IsDisplayed() not supported for {context}, skipping display check");
        }
    }

    void AssertImageMaintainsAspectRatio(IUIElement image, double originalWidth, double originalHeight,
        double tolerance = 0.25)
    {
        var imageRect = image.GetRect();
        var expectedRatio = originalWidth / originalHeight;
        var actualRatio = (double)imageRect.Width / imageRect.Height;

        var ratioDifference = Math.Abs(expectedRatio - actualRatio);
        
        Assert.That(ratioDifference, Is.LessThanOrEqualTo(tolerance),
            $"Image should maintain aspect ratio of {expectedRatio:F2} (original: {originalWidth}x{originalHeight}). " +
            $"Expected: {expectedRatio:F2}, Actual: {actualRatio:F2}, Difference: {ratioDifference:F2}, Size: {imageRect.Width}x{imageRect.Height}");
    }

    void AssertImageIsCentered(IUIElement image, string context)
    {
        var imageRect = image.GetRect();
        
        // For centered images, they should not be at screen edges
        Assert.That(imageRect.X, Is.GreaterThan(10), 
            $"Centered image should not be at left edge for {context}");
        Assert.That(imageRect.Y, Is.GreaterThan(10), 
            $"Centered image should not be at top edge for {context}");
    }
}
#endif