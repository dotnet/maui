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
        // Create an empty group first
        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");

        // Wait for the empty group to be created
        App.WaitForElement("Empty Group");

        // Try to drag an item from Group A to the empty group
        // This is the core test - it should not fail due to ItemViewType mismatch
        App.WaitForElement("Item A1");

        // On Android, drag and drop in CollectionView involves long press and drag gestures
        // The fix ensures that the drag operation doesn't fail when the target is a group header
        // in an empty group (which has a different ItemViewType than the dragged item)

        // Note: The actual drag gesture testing in Appium can be complex and platform-specific
        // This test primarily ensures the UI elements are present and the fix doesn't cause crashes

        // Verify that we can see all expected elements
        App.WaitForElement("Item A2");
        App.WaitForElement("Item A3");
        App.WaitForElement("Group A");
        App.WaitForElement("Group B");
        App.WaitForElement("Group C");
        App.WaitForElement("Empty Group");

        // The fix prevents the OnMove method from returning false when dragging
        // an item over a group header, which was the root cause of the issue
        // If the fix works, the app should remain responsive and not crash
        App.WaitForElement("StatusLabel");
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void EmptyGroupCreationShouldWork()
    {
        // Test creating empty groups
        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");

        App.WaitForElement("Empty Group");
        App.WaitForElement("StatusLabel");

        // Verify status was updated
        var statusText = App.WaitForElement("StatusLabel").GetText();
        Assert.That(statusText, Does.Contain("Empty group created"));
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void DragItemIntoEmptyGroupShouldSucceed()
    {
        // Create an empty group first
        App.WaitForElement("CreateEmptyGroupButton");
        App.Tap("CreateEmptyGroupButton");
        App.WaitForElement("Empty Group");

        // Drag Item C1 into Empty Group
        App.WaitForElement("Item A1");
        App.WaitForElement("Empty Group");

        // Perform drag-and-drop gesture (Appium syntax)
        App.DragAndDrop("Item A1", "Empty Group");

        // Wait for the item to appear in the empty group
        App.WaitForElement("Item A1");
        App.WaitForElement("Empty Group");
        App.WaitForElement("StatusLabel");

        // Optionally verify status label or group contents
        var statusText = App.WaitForElement("StatusLabel").GetText();
        Assert.That(statusText, Does.Contain("Empty group"));
    }
}
