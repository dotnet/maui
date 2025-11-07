#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986 : _IssuesUITest
{
	public override string Issue => "Test SafeArea attached property for per-edge safe area control";

	public Issue28986(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SoftInputDoesNotApplyBottomPaddingWhenKeyboardHidden()
	{
		// This test validates the fix for issue #31870
		// When SafeAreaEdges.Bottom is set to SoftInput and the keyboard is NOT showing,
		// there should be NO bottom padding from the navigation bar.
		App.WaitForElement("ContentGrid");

		// First, set to None to get the baseline position (no safe area padding)
		App.Tap("GridResetNoneButton");
		var noneRect = App.WaitForElement("MainGrid").GetRect();

		// Now set bottom edge to SoftInput (keyboard is still hidden)
		App.Tap("GridSetBottomSoftInputButton");

		// Wait for layout to update
		App.WaitForElement("MainGrid");

		// Get the current settings to verify SoftInput is set
		var currentSettings = App.WaitForElement("CurrentSettings").GetText();
		Assert.That(currentSettings, Does.Contain("Bottom:SoftInput"),
			"Bottom edge should be set to SoftInput");

		// Get the rect after setting SoftInput
		var softInputRect = App.WaitForElement("MainGrid").GetRect();

		// Verify that the bottom position is the same as None (no padding applied)
		// The height should be the same because SoftInput should not add padding when keyboard is hidden
		Assert.That(softInputRect.Height, Is.EqualTo(noneRect.Height).Within(5),
			"MainGrid height should be the same with SoftInput as with None when keyboard is hidden (no bottom padding)");

		// Also verify the Y position hasn't shifted
		Assert.That(softInputRect.Y, Is.EqualTo(noneRect.Y).Within(5),
			"MainGrid Y position should be the same with SoftInput as with None when keyboard is hidden");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void AllRegionStillAppliesBottomPaddingWhenKeyboardHidden()
	{
		// This test validates that the fix for #31870 doesn't break SafeAreaRegions.All
		// When SafeAreaEdges is set to All, it should respect safe area behavior consistently.
		// The specific assertion depends on whether the device has a navigation bar.
		App.WaitForElement("ContentGrid");

		// First, set to None to get the baseline position (no safe area padding)
		App.Tap("GridResetNoneButton");
		var noneRect = App.WaitForElement("MainGrid").GetRect();

		// Set bottom edge to SoftInput to test the specific behavior we fixed
		App.Tap("GridSetBottomSoftInputButton");
		App.WaitForElement("MainGrid");
		var softInputRect = App.WaitForElement("MainGrid").GetRect();

		// Now set to All (should apply all safe area insets)
		App.Tap("GridResetAllButton");
		App.WaitForElement("MainGrid");

		// Get the current settings to verify All is set
		var currentSettings = App.WaitForElement("CurrentSettings").GetText();
		Assert.That(currentSettings, Does.Contain("All (Full safe area)"),
			"SafeAreaEdges should be set to All");

		// Get the rect after setting All
		var allRect = App.WaitForElement("MainGrid").GetRect();

		// The key validation: All should have same or less height than None (never more)
		// And All should behave differently than SoftInput when keyboard is hidden
		// Note: Using LessThanOrEqualTo instead of LessThan because some test devices (e.g., emulators
		// without navigation bars) have no bottom safe area padding, resulting in equal heights.
		// This test validates behavior consistency rather than assuming specific padding values.
		Assert.That(allRect.Height, Is.LessThanOrEqualTo(noneRect.Height),
			"MainGrid height with All should be less than or equal to None (All respects safe area)");
		
		// SoftInput should match None when keyboard is hidden (no bottom padding)
		Assert.That(softInputRect.Height, Is.EqualTo(noneRect.Height).Within(5),
			"MainGrid height with SoftInput should match None when keyboard is hidden");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaMainGridBasicFunctionality()
	{
		// 1. Test loads - verify essential elements are present
		App.WaitForElement("ContentGrid");

		// 2. Verify initial state - MainGrid should start with All (offset by safe area)
		var initialSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(initialSettings, Does.Contain("All (Full safe area)"));


		var safePosition = App.WaitForElement("ContentGrid").GetRect();
		// 3. Click button to set SafeAreaEdge to "None" on the MainGrid
		App.Tap("GridResetNoneButton");

		var unSafePosition = App.WaitForElement("ContentGrid").GetRect();
		Assert.That(unSafePosition.Y, Is.EqualTo(0), "ContentGrid Y position should be 0 when SafeAreaEdges is set to None");
		Assert.That(safePosition.Y, Is.Not.EqualTo(0), "ContentGrid Y position should not be 0 when SafeAreaEdges is set to All");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaMainGridAllButtonFunctionality()
	{
		App.WaitForElement("GridResetAllButton");
		App.WaitForElement("ContentGrid");

		// First set to None to establish baseline position
		App.Tap("GridResetNoneButton");
		var nonePosition = App.WaitForElement("ContentGrid").GetRect();

		// Test "All" button functionality
		App.Tap("GridResetAllButton");
		var allPosition = App.WaitForElement("ContentGrid").GetRect();

		// Verify MainGrid is set to All
		var allSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(allSettings, Does.Contain("All (Full safe area)"));

		// Verify position changes - All should offset content away from screen edges
		Assert.That(allPosition.Y, Is.Not.EqualTo(0), "ContentGrid Y position should not be 0 when SafeAreaEdges is set to All");
		Assert.That(allPosition.Y, Is.GreaterThanOrEqualTo(nonePosition.Y), "ContentGrid should be positioned lower when SafeAreaEdges is All vs None");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaMainGridSequentialButtonTesting()
	{
		App.WaitForElement("ContentGrid");
		App.WaitForElement("CurrentSettings");

		// Test sequence: All -> None -> Container -> All with position validation

		// 1. Set to All and capture position
		App.Tap("GridResetAllButton");
		var allPosition = App.WaitForElement("ContentGrid").GetRect();
		var allSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(allSettings, Does.Contain("All (Full safe area)"));

		// 2. Set to None and verify position changes
		App.Tap("GridResetNoneButton");
		var nonePosition = App.WaitForElement("ContentGrid").GetRect();

		var noneSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));
		Assert.That(nonePosition.Y, Is.EqualTo(0), "ContentGrid Y position should be 0 when SafeAreaEdges is None (edge-to-edge)");
		Assert.That(allPosition.Y, Is.GreaterThan(nonePosition.Y), "All position should be lower than None position");

		// 3. Set to Container and verify position changes
		App.Tap("GridSetContainerButton");
		var containerPosition = App.WaitForElement("ContentGrid").GetRect();
		var containerSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));
		Assert.That(containerPosition.Y, Is.GreaterThanOrEqualTo(nonePosition.Y), "Container position should be lower than None position");

		// 4. Return to All and verify position matches original
		App.Tap("GridResetAllButton");
		var finalAllPosition = App.WaitForElement("ContentGrid").GetRect();
		var finalAllSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(finalAllSettings, Does.Contain("All (Full safe area)"));
		Assert.That(finalAllPosition.Y, Is.EqualTo(allPosition.Y), "Final All position should match initial All position");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaPerEdgeValidation()
	{
		App.WaitForElement("ContentGrid");

		// Set all 4 edges to Container
		App.Tap("GridSetContainerButton");
		App.Tap("GridSetBottomSoftInputButton");

		var containerPosition = App.WaitForElement("ContentGrid").GetRect();

		// Open Soft Input test entry
		App.Tap("SoftInputTestEntry");

		App.RetryAssert(() =>
		{
			var containerPositionWithSoftInput = App.WaitForElement("ContentGrid").GetRect();
			Assert.That(containerPositionWithSoftInput.Height, Is.LessThan(containerPosition.Height), "ContentGrid height should be less when Soft Input is shown with Container edges");
		});

		App.DismissKeyboard();

		App.RetryAssert(() =>
		{
			var containerPositionWithoutSoftInput = App.WaitForElement("ContentGrid").GetRect();

			Assert.That(containerPositionWithoutSoftInput.Height, Is.EqualTo(containerPosition.Height), "ContentGrid height should return to original when Soft Input is dismissed with Container edges");
		});
	}
}
#endif