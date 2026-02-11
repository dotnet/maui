using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33324 : _IssuesUITest
{
    public override string Issue => "CollectionView.EmptyView does not remeasure its height when the parent layout changes dynamically";

    public Issue33324(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void EmptyViewShouldRemeasureWhenParentLayoutChanges()
    {
        App.WaitForElement("LoadItemsButton");
        App.Tap("LoadItemsButton");
        App.WaitForElement("SecondCollectionView");
        VerifyScreenshot();
    }
}
