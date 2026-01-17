using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31814 : _IssuesUITest
{
    public Issue31814(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "CollectionView RemainingItemsThreshold incorrectly including Header and Footer in counts";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void AdjustRemainingItemsThresholdForHeaderFooter()
    {
        App.WaitForElement("Issue31814CollectionView");
        App.ScrollDown("Issue31814CollectionView", ScrollStrategy.Gesture, 0.99);
        App.WaitForElement("RemainingItemsThresholdReached fired");
    }
}