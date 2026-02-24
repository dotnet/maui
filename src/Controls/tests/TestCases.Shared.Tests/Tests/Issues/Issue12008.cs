# if TEST_FAILS_ON_WINDOWS  && TEST_FAILS_ON_ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12008 : _IssuesUITest
{
	public override string Issue => "CollectionView Drag and Drop Reordering Can't Drop in Empty Group";

	public Issue12008(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CanDragItemIntoEmptyGroup()
	{
		// Wait for the CollectionView to be ready
		App.WaitForElement("ReorderCollectionView");

		// Verify initial setup - items and empty group are present
		App.WaitForElement("Item A1");
		App.WaitForElement("Empty Group");

		// Drag an item from Group A into the Empty Group
		App.DragAndDrop("Item A1", "Empty Group");

		// Verify the reorder completed successfully by checking status label
		var statusLabel = App.WaitForElement("StatusLabel");
		var statusText = statusLabel.GetText();
		Assert.That(statusText, Does.Contain("Reorder completed"), "Status should show reorder completed after dragging into empty group");
	}
}
#endif