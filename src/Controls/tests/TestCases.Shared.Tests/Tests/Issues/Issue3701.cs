using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3701 : _IssuesUITest
	{
		const string VisibleItemId = "VisibleItem";
		const string HiddenItemId = "HiddenItem";
		const string ToggleItemId = "ToggleItem";
		const string ToggleHiddenButtonId = "ToggleHiddenButton";
		const string ToggleSecondaryButtonId = "ToggleSecondaryButton";
		const string StatusLabelId = "StatusLabel";

		public Issue3701(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add IsVisible Property to ToolbarItem";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemIsVisibleInitialState()
		{
			// Check that visible item is present and hidden item is not
			App.WaitForElement(VisibleItemId);
			
			// Try to find hidden item - it should not be present
			var hiddenElements = App.FindElements(HiddenItemId);
			Assert.IsEmpty(hiddenElements, "Hidden toolbar item should not be visible initially");
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemIsVisibleCanBeToggled()
		{
			// Initially hidden item should not be visible
			var hiddenElements = App.FindElements(HiddenItemId);
			Assert.IsEmpty(hiddenElements, "Hidden toolbar item should not be visible initially");

			// Toggle visibility
			App.WaitForElement(ToggleHiddenButtonId);
			App.Tap(ToggleHiddenButtonId);

			// Now hidden item should be visible
			App.WaitForElement(HiddenItemId);
			
			// Check status label
			var statusLabel = App.WaitForElement(StatusLabelId);
			Assert.IsTrue(statusLabel.GetText().Contains("True"), "Status should show Hidden Item IsVisible: True");

			// Toggle back to hidden
			App.Tap(ToggleHiddenButtonId);
			
			// Hidden item should be gone again
			hiddenElements = App.FindElements(HiddenItemId);
			Assert.IsEmpty(hiddenElements, "Hidden toolbar item should not be visible after toggling back");
			
			// Check status label
			statusLabel = App.WaitForElement(StatusLabelId);
			Assert.IsTrue(statusLabel.GetText().Contains("False"), "Status should show Hidden Item IsVisible: False");
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemSecondaryIsVisibleCanBeToggled()
		{
			// Initially secondary item should be visible
			App.WaitForElement(ToggleItemId);

			// Toggle visibility
			App.WaitForElement(ToggleSecondaryButtonId);
			App.Tap(ToggleSecondaryButtonId);

			// Now secondary item should be hidden
			var toggleElements = App.FindElements(ToggleItemId);
			Assert.IsEmpty(toggleElements, "Toggle toolbar item should not be visible after hiding");
			
			// Check status label
			var statusLabel = App.WaitForElement(StatusLabelId);
			Assert.IsTrue(statusLabel.GetText().Contains("False"), "Status should show Toggle Item IsVisible: False");

			// Toggle back to visible
			App.Tap(ToggleSecondaryButtonId);
			
			// Secondary item should be visible again
			App.WaitForElement(ToggleItemId);
			
			// Check status label
			statusLabel = App.WaitForElement(StatusLabelId);
			Assert.IsTrue(statusLabel.GetText().Contains("True"), "Status should show Toggle Item IsVisible: True");
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemIsVisibleWorksWithMultipleItems()
		{
			// Verify initial state: visible item present, hidden item absent
			App.WaitForElement(VisibleItemId);
			var hiddenElements = App.FindElements(HiddenItemId);
			Assert.IsEmpty(hiddenElements, "Hidden toolbar item should not be visible initially");
			
			App.WaitForElement(ToggleItemId);

			// Show the hidden item
			App.Tap(ToggleHiddenButtonId);
			
			// Both visible and previously hidden items should now be present
			App.WaitForElement(VisibleItemId);
			App.WaitForElement(HiddenItemId);
			App.WaitForElement(ToggleItemId);
			
			// Hide the secondary item
			App.Tap(ToggleSecondaryButtonId);
			
			// Visible and hidden (now shown) should be present, toggle should be absent
			App.WaitForElement(VisibleItemId);
			App.WaitForElement(HiddenItemId);
			var toggleElements = App.FindElements(ToggleItemId);
			Assert.IsEmpty(toggleElements, "Toggle toolbar item should not be visible after hiding");
		}
	}
}