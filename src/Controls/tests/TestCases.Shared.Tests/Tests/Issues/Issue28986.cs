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
			App.WaitForElement("CurrentSettings");
			App.WaitForElement("GridResetNoneButton");
			App.WaitForElement("GridSetContainerButton");
			App.WaitForElement("GridResetAllButton");

			// 2. Verify initial state - MainGrid should start with All (offset by safe area)
			var initialSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(initialSettings, Does.Contain("All (Full safe area)"));

			// 3. Click button to set SafeAreaEdge to "None" on the MainGrid
			App.Tap("GridResetNoneButton");

			// 4. Verify it's flush with the screen and has zero offset
			var noneSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));

			// 5. Click a button to set it to Container
			App.Tap("GridSetContainerButton");

			// 6. Verify that it is now offset again by the safe area
			var containerSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));
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