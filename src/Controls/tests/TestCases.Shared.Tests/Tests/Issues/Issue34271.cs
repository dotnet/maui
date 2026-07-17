using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34271 : _IssuesUITest
{
	public Issue34271(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView scroll position resets to top after ScrollTo last item when Picker is dismissed";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewScrollPositionPreservedAfterPickerDismiss()
	{
		// Scroll CollectionView to the last item (Proboscis Monkey)
		App.WaitForElement("ScrollButton");
		App.Tap("ScrollButton");

		// Verify last item is now visible
		App.WaitForElement("Proboscis Monkey");
		var rectBeforePicker = App.WaitForElement("Proboscis Monkey").GetRect();

		// Open the picker (triggers the bug on Mac Catalyst via TraitCollectionDidChange)
		App.Tap("PositionPicker");

		// Dismiss the picker (App.ClosePicker handles all platforms:
		// Android=Cancel button, iOS/Mac=Done button, Windows=TapCoordinates)
		App.ClosePicker(windowsTapx: 10, windowsTapy: 10);

		// Verify scroll position was preserved — last item still visible at same position
		App.WaitForElement("Proboscis Monkey");
		var rectAfterPicker = App.WaitForElement("Proboscis Monkey").GetRect();

		Assert.That(rectAfterPicker.Y, Is.EqualTo(rectBeforePicker.Y).Within(5),
			$"CollectionView scroll position reset after Picker dismiss. " +
			$"Before: Y={rectBeforePicker.Y}, After: Y={rectAfterPicker.Y}");

		// Trigger layout recomputation via InvalidateMeasure — forces UIKit to
		// recompute contentSize using estimated item sizes, which can silently
		// clamp contentOffset when varying-height items cause a large estimated/actual gap.
		App.Tap("TriggerLayoutButton");

		// Verify scroll position is still preserved after layout recomputation
		App.WaitForElement("Proboscis Monkey");
		var rectAfterLayout = App.WaitForElement("Proboscis Monkey").GetRect();

		Assert.That(rectAfterLayout.Y, Is.EqualTo(rectBeforePicker.Y).Within(5),
			$"CollectionView scroll position reset after layout recomputation. " +
			$"Before: Y={rectBeforePicker.Y}, After: Y={rectAfterLayout.Y}");
	}
}
