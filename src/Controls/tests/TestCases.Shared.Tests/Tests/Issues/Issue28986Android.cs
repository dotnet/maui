#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28986Android : _IssuesUITest
	{
		public override string Issue => "Test SafeArea attached property for per-edge safe area control on Android";

		public Issue28986Android(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaMainGridBasicFunctionalityAndroid()
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

			// On Android, verify that SafeAreaEdges None results in edge-to-edge layout
			Assert.That(unSafePosition.Y, Is.EqualTo(0), "ContentGrid Y position should be 0 when SafeAreaEdges is set to None");
			Assert.That(safePosition.Y, Is.Not.EqualTo(0), "ContentGrid Y position should not be 0 when SafeAreaEdges is set to All");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaMainGridAllButtonFunctionalityAndroid()
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
			Assert.That(allPosition.Y, Is.GreaterThan(nonePosition.Y), "ContentGrid should be positioned lower when SafeAreaEdges is All vs None");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaMainGridSequentialButtonTestingAndroid()
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
			Assert.That(containerPosition.Y, Is.GreaterThan(nonePosition.Y), "Container position should be lower than None position");

			// 4. Return to All and verify position matches original
			App.Tap("GridResetAllButton");
			var finalAllPosition = App.WaitForElement("ContentGrid").GetRect();
			var finalAllSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(finalAllSettings, Does.Contain("All (Full safe area)"));
			Assert.That(finalAllPosition.Y, Is.EqualTo(allPosition.Y), "Final All position should match initial All position");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaPerEdgeValidationAndroid()
		{
			App.WaitForElement("ContentGrid");

			// Test per-edge functionality specifically for Android
			// First establish baseline with Container setting
			App.Tap("GridSetContainerButton");
			var containerPosition = App.WaitForElement("ContentGrid").GetRect();

			// Test top edge setting to None - should allow content into status bar area
			App.Tap("GridSetTopNoneButton");
			var topNonePosition = App.WaitForElement("ContentGrid").GetRect();
			
			// Verify the Y position is different when top edge is set to None
			Assert.That(topNonePosition.Y, Is.LessThan(containerPosition.Y), "Content should move higher when top edge is set to None");

			// Test bottom edge SoftInput behavior
			App.Tap("GridSetBottomSoftInputButton");
			var currentSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(currentSettings, Does.Contain("SoftInput"), "Current settings should show SoftInput for bottom edge");

			// Test keyboard interaction for SoftInput
			App.Tap("SoftInputTestEntry");
			var entryFocusedPosition = App.WaitForElement("ContentGrid").GetRect();
			
			// Android specific: Verify layout adjusts when keyboard is shown with SoftInput edge
			// Note: Height comparison is more reliable than Y position on Android
			Assert.That(entryFocusedPosition.Height, Is.LessThan(topNonePosition.Height), 
				"ContentGrid height should be less when keyboard is shown with SoftInput bottom edge");

			App.DismissKeyboard();
			
			var keyboardDismissedPosition = App.WaitForElement("ContentGrid").GetRect();
			Assert.That(keyboardDismissedPosition.Height, Is.GreaterThan(entryFocusedPosition.Height), 
				"ContentGrid height should increase when keyboard is dismissed");
		}
	}
}
#endif