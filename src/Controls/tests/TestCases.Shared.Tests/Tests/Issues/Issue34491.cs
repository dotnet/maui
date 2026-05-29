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
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("StatusLabel");

		var initialText = App.FindElement("StatusLabel").GetText() ?? string.Empty;
		Assert.That(initialText, Is.EqualTo("No Selection"));

		App.WaitForElement("Item 1");
		App.Tap("Item 1");

		App.WaitForTextToBePresentInElement("StatusLabel", "Selected: Item 1");

		var finalText = App.FindElement("StatusLabel").GetText() ?? string.Empty;

		Assert.That(finalText, Is.EqualTo("Selected: Item 1"),
			"SelectionChanged should fire when tapping a CollectionView item that has a PointerGestureRecognizer");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void PointerPressedAndReleasedStillFire()
	{
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("PointerStatusLabel");

		var initialText = App.FindElement("PointerStatusLabel").GetText() ?? string.Empty;
		Assert.That(initialText, Is.EqualTo("No Pointer Events"));

		App.WaitForElement("Item 1");
		App.Tap("Item 1");

		App.WaitForTextToBePresentInElement("PointerStatusLabel", "Pointer Released: Item 1");

		var finalText = App.FindElement("PointerStatusLabel").GetText() ?? string.Empty;

		Assert.That(finalText, Is.EqualTo("Pointer Released: Item 1"),
			"PointerPressed/PointerReleased should still fire after fix");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void MixedTapAndPointerGesturesStillAllowSelectionAndTap()
	{
		App.WaitForElement("MixedTestCollectionView");
		App.WaitForElement("MixedSelectionStatusLabel");
		App.WaitForElement("MixedTapStatusLabel");

		App.WaitForElement("Mixed Item 1");
		App.Tap("Mixed Item 1");

		App.WaitForTextToBePresentInElement("MixedSelectionStatusLabel", "Mixed Selected: Mixed Item 1");
		App.WaitForTextToBePresentInElement("MixedTapStatusLabel", "Tapped: Mixed Item 1");

		var selectionText = App.FindElement("MixedSelectionStatusLabel").GetText() ?? string.Empty;
		var tapText = App.FindElement("MixedTapStatusLabel").GetText() ?? string.Empty;

		Assert.That(selectionText, Is.EqualTo("Mixed Selected: Mixed Item 1"),
			"SelectionChanged should still fire when tap and pointer gestures coexist");

		Assert.That(tapText, Is.EqualTo("Tapped: Mixed Item 1"),
			"TapGestureRecognizer should still fire when pointer gestures are present");
	}
}
#endif
