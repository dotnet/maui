using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32702 : _IssuesUITest
{
	public Issue32702(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView item selection doesn't work when DragGestureRecognizer or DropGestureRecognizer is attached to item content";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionWorksWithDragDropGestures()
	{
		// Wait for CollectionView to load
		App.WaitForElement("TestCollectionView");

		// Verify initial state
		var statusLabel = App.WaitForElement("StatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("No selection"));

		// Tap on first item
		App.Tap("Item 1");

		// Verify selection changed
		Assert.That(statusLabel.GetText(), Is.EqualTo("Selected: Item 1"));

		// Tap on second item
		App.Tap("Item 2");

		// Verify selection changed again
		Assert.That(statusLabel.GetText(), Is.EqualTo("Selected: Item 2"));
	}
}