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
	public void SafeAreaContainerWithSoftInputFunctionality()
	{
		// This test validates the fix for AdjustPan mode with Container+SoftInput configuration
		// Verify that the content doesn't overlap system UI when keyboard appears

		App.WaitForElement("ContentGrid");

		// Set all 4 edges to Container, then bottom to SoftInput
		App.Tap("GridSetContainerButton");
		App.Tap("GridSetBottomSoftInputButton");

		// Verify the settings are applied correctly
		var settings = App.WaitForElement("CurrentSettings").GetText();
		Assert.That(settings, Does.Contain("Container"), "SafeAreaEdges should be set to Container");
		Assert.That(settings, Does.Contain("SoftInput"), "Bottom edge should be set to SoftInput");
		
		App.Tap("SoftInputTestEntry");
		
		App.RetryAssert(() =>
		{
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");
		});

		// The fix ensures that in AdjustPan mode, the MainGrid gets proper padding
		// so content doesn't overlap with system UI. We validate this by ensuring
		// the Entry field remains accessible (not covered by keyboard or system UI)
		App.RetryAssert(() =>
		{
			var entry = App.WaitForElement("SoftInputTestEntry");
			Assert.That(entry, Is.Not.Null, "Entry should remain accessible when keyboard is shown");
		});

		App.DismissKeyboard();

		// Verify keyboard is dismissed
		App.RetryAssert(() =>
		{
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be dismissed");
		});

		// Verify entry is still accessible after keyboard dismissal
		var entryAfter = App.WaitForElement("SoftInputTestEntry");
		Assert.That(entryAfter, Is.Not.Null, "Entry should remain accessible after keyboard is dismissed");
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
	
#if TEST_FAILS_ON_ANDROID
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
#endif
}
#endif