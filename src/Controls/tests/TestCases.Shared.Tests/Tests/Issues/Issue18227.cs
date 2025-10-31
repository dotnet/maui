using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18227 : _IssuesUITest
{
    public override string Issue => "CollectionView with Header Causes ArgumentOutOfRangeException on reordering to last index";

    public Issue18227(TestDevice device)
    : base(device)
    { }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void DragFirstItemToLastPosition()
    {
        App.WaitForElement("collectionview");
        App.DragAndDrop("Test label 1", "Test label 6");
    }
}