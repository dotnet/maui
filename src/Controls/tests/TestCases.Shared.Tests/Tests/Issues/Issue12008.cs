#if TEST_FAILS_ON_WINDOWS // The test fails on Windows because Drag and Drop with grouping is not supported on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12008 : _IssuesUITest
{
    public Issue12008(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "CollectionView Drag and Drop Reordering Can't Drop in Empty Group";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void EmptyGroupCreationShouldWork()
    {
        App.WaitForElement("CreateEmptyGroupButton12008");
        App.Tap("CreateEmptyGroupButton12008");

        App.WaitForElement("GroupHeader12008EmptyGroup");
        App.WaitForElement("StatusLabel12008");
        var statusText = App.WaitForElement("StatusLabel12008").GetText();
        Assert.That(statusText, Does.Contain("Empty group created"));
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void DragItemIntoEmptyGroupShouldSucceed()
    {
        App.WaitForElement("CreateEmptyGroupButton12008");
        App.Tap("CreateEmptyGroupButton12008");
        App.WaitForElement("GroupHeader12008EmptyGroup");
        App.WaitForElement("Item12008ItemA1");
        App.DragAndDrop("Item12008ItemA1", "GroupHeader12008EmptyGroup");
        App.WaitForElement("GroupCount12008EmptyGroup");
        var groupCountLabel = App.WaitForElement("GroupCount12008EmptyGroup");
        var countText = groupCountLabel.GetText();
        Assert.That(countText, Is.EqualTo("Count: 1"), "Item was not moved into the empty group");
    }
}
#endif