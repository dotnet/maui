using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22120 : _IssuesUITest
{
    public Issue22120(TestDevice device) : base(device) { }

    public override string Issue => "CollectionView.Header is not scrollable in Android platform";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void CollectionViewHeaderScrollViewIsScrollable()
    {
        App.WaitForElement("Issue22120HeaderScrollView");
        App.WaitForElement("Header Item 1");
        App.ScrollDown("Issue22120HeaderScrollView", ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 1000);
        App.WaitForElement("Header Item 7", timeout: TimeSpan.FromSeconds(5));
        App.ScrollDown("Issue22120HeaderScrollView", ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 1000);
        App.WaitForElement("Header Item 12", timeout: TimeSpan.FromSeconds(5));
        App.ScrollUp("Issue22120HeaderScrollView", ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 1000);
        App.ScrollUp("Issue22120HeaderScrollView", ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 1000);
        App.WaitForElement("Header Item 1", timeout: TimeSpan.FromSeconds(5));
    }
}
