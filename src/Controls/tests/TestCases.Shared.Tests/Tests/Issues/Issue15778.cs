using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15778 : _IssuesUITest
{
    public Issue15778(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "CollectionView SelectionChanged gets fired when performing swipe using swipe view";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void SwipeViewInCollectionViewDoesNotTriggerSelection()
    {
        App.WaitForElement("Item 1");
        App.SwipeLeftToRight("Item 1");
        var label = App.WaitForElement("StatusLabel");
        Assert.That(label.GetText(), Is.EqualTo("SelectionChanged Not Triggered"));
    }
}
