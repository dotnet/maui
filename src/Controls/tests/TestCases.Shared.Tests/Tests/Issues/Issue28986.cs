#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28986 : _IssuesUITest
	{
		public override string Issue => "Test SafeArea attached property for per-edge safe area control";

		public Issue28986(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Layout)]
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
		[Category(UITestCategories.Layout)]
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
			Assert.That(allPosition.Y, Is.GreaterThan(nonePosition.Y), "ContentGrid should be positioned lower when SafeAreaEdges is All vs None");
		}

		[Test]
		[Category(UITestCategories.Layout)]
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
		public void SafeAreaPerEdgeValidation()
		{
			App.WaitForElement("ContentGrid");
			App.WaitForElement("CurrentSettings");

			// Test per-edge scenario: bottom edge to SoftInput, then top edge to Container, then top edge to None
			
			// 1. Set bottom edge to SoftInput
			App.Tap("GridSetBottomSoftInputButton");
			var bottomSoftInputSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(bottomSoftInputSettings, Does.Contain("Bottom:SoftInput"), "Should show bottom edge set to SoftInput");

			// 2. Set top edge to Container (while keeping bottom as SoftInput)
			App.Tap("GridSetTopContainerButton");
			var topContainerSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(topContainerSettings, Does.Contain("Top:Container"), "Should show top edge set to Container");
			Assert.That(topContainerSettings, Does.Contain("Bottom:SoftInput"), "Should maintain bottom edge as SoftInput");

			// 3. Set top edge to None (while keeping bottom as SoftInput)
			App.Tap("GridSetTopNoneButton");
			var topNoneSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(topNoneSettings, Does.Contain("Top:None"), "Should show top edge set to None");
			Assert.That(topNoneSettings, Does.Contain("Bottom:SoftInput"), "Should maintain bottom edge as SoftInput");
		}
	}
}
#endif