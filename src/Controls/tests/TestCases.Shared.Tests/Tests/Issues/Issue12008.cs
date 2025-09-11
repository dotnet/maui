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
    public void DragAndDropIntoEmptyGroupShouldWork()
    {

        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");
        App.WaitForElement("Empty Group");
        App.WaitForElement("Item A1");
        App.WaitForElement("Item A2");
        App.WaitForElement("Item A3");
        App.WaitForElement("Group A");
        App.WaitForElement("Group B");
        App.WaitForElement("Group C");
        App.WaitForElement("Empty Group");
        App.WaitForElement("StatusLabel");
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void EmptyGroupCreationShouldWork()
    {
        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");

        App.WaitForElement("Empty Group");
        App.WaitForElement("StatusLabel");
        var statusText = App.WaitForElement("StatusLabel").GetText();
        Assert.That(statusText, Does.Contain("Empty group created"));
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void DragItemIntoEmptyGroupShouldSucceed()
    {
        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");
        App.WaitForElement("Empty Group");
        App.WaitForElement("Item A1");
        App.WaitForElement("Empty Group");
        App.DragAndDrop("Item A1", "Empty Group");
        App.WaitForElement("Item A1");
        App.WaitForElement("Empty Group");
        App.WaitForElement("StatusLabel");
        var statusText = App.WaitForElement("StatusLabel").GetText();
        Assert.That(statusText, Does.Contain("Empty group"));
    }
}
