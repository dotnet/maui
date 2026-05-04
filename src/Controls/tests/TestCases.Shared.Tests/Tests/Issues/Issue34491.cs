#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34491 : _IssuesUITest
{
	public Issue34491(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView item selection not triggered when PointerGestureRecognizer is added inside ItemTemplate";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionWorksWithPointerGestureRecognizer()
	{
		// Wait for CollectionView
		App.WaitForElement("TestCollectionView");

		// Ensure StatusLabel exists
		App.WaitForElement("StatusLabel");

		var initialText = App.FindElement("StatusLabel").GetText() ?? string.Empty;
		Assert.That(initialText, Is.EqualTo("No Selection"));

		// Tap item
		App.WaitForElement("Item 1");
		App.Tap("Item 1");

		App.WaitForTextToBePresentInElement("StatusLabel", "Selected: Item 1");

		var finalText = App.FindElement("StatusLabel").GetText() ?? string.Empty;

		Assert.That(finalText, Is.EqualTo("Selected: Item 1"),
			"SelectionChanged should fire when tapping a CollectionView item that has a PointerGestureRecognizer");
	}
}
#endif
