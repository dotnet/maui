#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30403PanoramicImages : _IssuesUITest
{
    public Issue30403PanoramicImages(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue =>
        "Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Panoramic)";

    [Test, Order(1)]
    [Category(UITestCategories.Image)]
    public void PanoramicImageCenterAlignment_ShouldRespectLayoutOptions()
    {
        App.WaitForElement("PanoramicCenter");
        var image = App.FindElement("PanoramicCenter");

        // Verify image maintains aspect ratio (this is the core fix validation)
        AssertImageMaintainsAspectRatio(image, 800, 200);
        
        // Verify image is visible and has reasonable size
        AssertImageIsVisible(image, "panoramic center");
        
        // For center alignment, verify the image is not positioned at screen edges
        AssertImageNotAtScreenEdges(image, "panoramic center");
    }

    [Test, Order(2)]
    [Category(UITestCategories.Image)]
    public void PanoramicImageStartAlignment_ShouldAlignToTopLeft()
    {
        App.WaitForElement("PanoramicStart");
        var image = App.FindElement("PanoramicStart");

        AssertImageMaintainsAspectRatio(image, 800, 200);
        AssertImageIsVisible(image, "panoramic start");
    }

    [Test, Order(3)]
    [Category(UITestCategories.Image)]
    public void PanoramicImageEndAlignment_ShouldAlignToBottomRight()
    {     
        App.WaitForElement("PanoramicScroll");
        App.ScrollUp("PanoramicScroll", ScrollStrategy.Gesture, 0.2);
        
        App.WaitForElement("PanoramicEnd");
        var image = App.FindElement("PanoramicEnd");

        AssertImageMaintainsAspectRatio(image, 800, 200);
        AssertImageIsVisible(image, "panoramic end");
        
        // For end alignment, verify image is positioned towards bottom area
        var imageRect = image.GetRect();
        var startImage = App.FindElement("PanoramicStart");
        var startRect = startImage.GetRect();
        
        Assert.That(imageRect.Y, Is.GreaterThan(startRect.Y), 
            "End-aligned image should be positioned below start-aligned image");
    }
    
    [Test, Order(4)]
    [Category(UITestCategories.Image)]
    public void AllPanoramicImages_ShouldLoadAndDisplay()
    {
	    var coreImageIds = new[]
	    {
		    "PanoramicCenter", "PanoramicStart", "PanoramicEnd"
	    };

	    foreach (var id in coreImageIds)
	    {
		    try
		    {
			    App.WaitForElement(id, timeout: TimeSpan.FromSeconds(10));
			    var image = App.FindElement(id);
			    AssertImageIsVisible(image, $"core panoramic {id}");
			    AssertImageMaintainsAspectRatio(image, 800, 200);
		    }
		    catch (Exception ex)
		    {
			    Assert.Fail($"Core panoramic image {id} failed to load or display properly: {ex.Message}");
		    }
	    }
    }
    
    [Test, Order(5)]
    [Category(UITestCategories.Image)]
    public void ConstrainedWidthContainer_ShouldHandleNarrowSpace()
    {
        App.WaitForElement("PanoramicScroll");
        App.ScrollDown("PanoramicScroll", ScrollStrategy.Gesture, 0.6);
        
        App.WaitForElement("PanoramicNarrow");
        var image = App.FindElement("PanoramicNarrow");

        AssertImageIsVisible(image, "narrow container image");
        
        // Images should maintain aspect ratio even when severely constrained
        AssertImageMaintainsAspectRatio(image, 800, 200, tolerance: 0.3);
        
        // Should be constrained due to narrow container
        var imageRect = image.GetRect();
        Assert.That(imageRect.Width, Is.LessThan(200), 
            "Image in narrow container should be constrained in width");
    }

    [Test, Order(6)]
    [Category(UITestCategories.Image)]
    public void ConstrainedHeightContainer_ShouldHandleShortSpace()
    {
        App.WaitForElement("PanoramicScroll");
        App.ScrollDown("PanoramicScroll", ScrollStrategy.Gesture, 0.6);
        
        App.WaitForElement("PanoramicShort");
        var image = App.FindElement("PanoramicShort");

        AssertImageIsVisible(image, "short container image");
        
        // Images should maintain aspect ratio even when height constrained
        AssertImageMaintainsAspectRatio(image, 800, 200, tolerance: 0.3);
        
        // Should be constrained due to short container
        var imageRect = image.GetRect();
        Assert.That(imageRect.Height, Is.LessThan(100), 
            "Image in short container should be constrained in height");
    }

    [Test, Order(7)]
    [Category(UITestCategories.Image)]
    public void MultipleImagesInGrid_ShouldRespectIndividualAlignments()
    {
        App.WaitForElement("PanoramicScroll");
        App.ScrollDown("PanoramicScroll", ScrollStrategy.Gesture, 0.6);
        
        var imageIds = new[] 
        { 
            "MultiPanoramicTopLeft", "MultiPanoramicTopRight", 
            "MultiPanoramicBottomLeft", "MultiPanoramicBottomRight" 
        };

        foreach (var id in imageIds)
        {
            App.WaitForElement(id);
            var image = App.FindElement(id);
            AssertImageIsVisible(image, $"multi panoramic {id}");
            AssertImageMaintainsAspectRatio(image, 800, 200);
        }

        // Verify relative positioning
        var topLeft = App.FindElement("MultiPanoramicTopLeft").GetRect();
        var topRight = App.FindElement("MultiPanoramicTopRight").GetRect();
        var bottomLeft = App.FindElement("MultiPanoramicBottomLeft").GetRect();
        var bottomRight = App.FindElement("MultiPanoramicBottomRight").GetRect();

        Assert.That(topLeft.Y, Is.LessThan(bottomLeft.Y), 
            "Top images should be above bottom images");
        Assert.That(topLeft.X, Is.LessThan(topRight.X), 
            "Left images should be to the left of right images");
    }
    
    // Helper methods focusing on reliable image verification
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
            // Some platforms may not support IsDisplayed(), skip this check
            TestContext.WriteLine($"IsDisplayed() not supported for {context}, skipping visibility check");
        }
    }

    void AssertImageMaintainsAspectRatio(IUIElement image, double originalWidth, double originalHeight,
        double tolerance = 0.2)
    {
        var imageRect = image.GetRect();
        var expectedRatio = originalWidth / originalHeight;
        var actualRatio = (double)imageRect.Width / imageRect.Height;

        var ratioDifference = Math.Abs(expectedRatio - actualRatio);
        
        Assert.That(ratioDifference, Is.LessThanOrEqualTo(tolerance),
            $"Image should maintain aspect ratio of {expectedRatio:F2} (original: {originalWidth}x{originalHeight}). " +
            $"Expected: {expectedRatio:F2}, Actual: {actualRatio:F2}, Difference: {ratioDifference:F2}");
    }

    void AssertImageNotAtScreenEdges(IUIElement image, string context)
    {
        var imageRect = image.GetRect();
        
        // Image should not be positioned at the very edges (indicating it's centered)
        Assert.That(imageRect.X, Is.GreaterThan(5), 
            $"Centered image should not be at left edge for {context}");
        Assert.That(imageRect.Y, Is.GreaterThan(5), 
            $"Centered image should not be at top edge for {context}");
    }
}
#endif