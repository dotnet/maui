using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38275 : _IssuesUITest
{
    public Issue38275(TestDevice device)
        : base(device)
    {
    }

    public override string Issue => "CollectionView Label renders truncated after ItemsSource is swapped with MeasureFirstItem";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void LabelsShouldNotBeTruncatedAfterItemsSourceIsReplaced()
    {
        App.WaitForElement("Issue38275Date0");

        for (int swapCount = 1; swapCount <= 3; swapCount++)
        {
            App.Tap("Issue38275SwapButton");
            App.WaitForTextToBePresentInElement("Issue38275StatusLabel", $"Swap count: {swapCount}");
        }

        VerifyScreenshot();
    }
}