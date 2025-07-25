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
			App.WaitForElement("CurrentSettings");

			// Test "All" button functionality
			App.Tap("GridResetAllButton");

			// Verify MainGrid is set to All
			var allSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allSettings, Does.Contain("All (Full safe area)"));
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaMainGridSequentialButtonTesting()
		{
			App.WaitForElement("CurrentSettings");

			// Test sequence: All -> None -> Container -> All
			App.Tap("GridResetAllButton");
			var allSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allSettings, Does.Contain("All (Full safe area)"));

			App.Tap("GridResetNoneButton");
			var noneSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));

			App.Tap("GridSetContainerButton");
			var containerSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));

			App.Tap("GridResetAllButton");
			var finalAllSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(finalAllSettings, Does.Contain("All (Full safe area)"));
		}
	}
}