#if TEST_FAILS_ON_WINDOWS   // Scrolling the CollectionView using App.ScrollDown is not working on Windows platform.  
                            // Also, the issue itself is not reproducible on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31680 : _IssuesUITest
{
    public Issue31680(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "System.IndexOutOfRangeException when scrolling CollectionView with image CarouselView";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void CollectionViewInsideCarouselViewShouldNotThrowIndexOutOfRangeException()
    {
        App.WaitForElement("TitleLabel");

        // Note: This issue occurs only during manual scrolling. The test may fail randomly without the fix, 
        // but the issue can be consistently reproduced manually with each scroll.
        App.ScrollDown("MainCollectionView", ScrollStrategy.Gesture, 0.99, swipeSpeed: 50, true);
        App.ScrollDown("MainCollectionView", ScrollStrategy.Gesture, 0.99, swipeSpeed: 50, true);
#if MACCATALYST
        // MacCatalyst needs more scroll gestures to reach the bottom.
        for (int i = 0; i < 5; i++)
        {
            App.ScrollDown("MainCollectionView", ScrollStrategy.Gesture, 0.99, swipeSpeed: 50, true);

            if (App.FindElements("Footer").Count > 0)
                break;
        }
#endif
        App.ScrollTo("Footer");
        App.WaitForElement("Footer");
    }
}
#endif