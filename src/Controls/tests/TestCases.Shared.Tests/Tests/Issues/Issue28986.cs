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
		public void SafeAreaInitialStateIsCorrect()
		{
			// Verify initial state - all checkboxes should be unchecked
			App.WaitForElement("TopCheckBox");
			App.WaitForElement("LeftCheckBox");
			App.WaitForElement("RightCheckBox");
			App.WaitForElement("BottomCheckBox");

			// Verify all checkboxes start unchecked (default behavior)
			Assert.That(App.FindElement("TopCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("LeftCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("RightCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("BottomCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));

			// Verify initial settings label contains all "Respect"
			App.WaitForElement("CurrentSettings");
			var initialSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(initialSettings, Does.Contain("Left: Respect"));
			Assert.That(initialSettings, Does.Contain("Top: Respect"));
			Assert.That(initialSettings, Does.Contain("Right: Respect"));
			Assert.That(initialSettings, Does.Contain("Bottom: Respect"));
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaIndividualCheckboxFunctionality()
		{
			App.WaitForElement("TopCheckBox");
			App.WaitForElement("CurrentSettings");

			// Test Top checkbox
			App.Tap("TopCheckBox");
			var settingsAfterTop = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterTop, Does.Contain("Top: Ignore"));
			Assert.That(settingsAfterTop, Does.Contain("Left: Respect"));
			Assert.That(settingsAfterTop, Does.Contain("Right: Respect"));
			Assert.That(settingsAfterTop, Does.Contain("Bottom: Respect"));

			// Test Left checkbox
			App.Tap("LeftCheckBox");
			var settingsAfterLeft = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterLeft, Does.Contain("Top: Ignore"));
			Assert.That(settingsAfterLeft, Does.Contain("Left: Ignore"));
			Assert.That(settingsAfterLeft, Does.Contain("Right: Respect"));
			Assert.That(settingsAfterLeft, Does.Contain("Bottom: Respect"));

			// Test Right checkbox
			App.Tap("RightCheckBox");
			var settingsAfterRight = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterRight, Does.Contain("Top: Ignore"));
			Assert.That(settingsAfterRight, Does.Contain("Left: Ignore"));
			Assert.That(settingsAfterRight, Does.Contain("Right: Ignore"));
			Assert.That(settingsAfterRight, Does.Contain("Bottom: Respect"));

			// Test Bottom checkbox
			App.Tap("BottomCheckBox");
			var settingsAfterBottom = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterBottom, Does.Contain("Top: Ignore"));
			Assert.That(settingsAfterBottom, Does.Contain("Left: Ignore"));
			Assert.That(settingsAfterBottom, Does.Contain("Right: Ignore"));
			Assert.That(settingsAfterBottom, Does.Contain("Bottom: Ignore"));
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaResetAllButtonFunctionality()
		{
			App.WaitForElement("ResetAllButton");
			App.WaitForElement("CurrentSettings");

			// Tap Reset All button
			App.Tap("ResetAllButton");

			// Verify all checkboxes are checked
			Assert.That(App.FindElement("TopCheckBox").GetAttribute<string>("checked"), Is.EqualTo("true"));
			Assert.That(App.FindElement("LeftCheckBox").GetAttribute<string>("checked"), Is.EqualTo("true"));
			Assert.That(App.FindElement("RightCheckBox").GetAttribute<string>("checked"), Is.EqualTo("true"));
			Assert.That(App.FindElement("BottomCheckBox").GetAttribute<string>("checked"), Is.EqualTo("true"));

			// Verify settings label shows all "Ignore"
			var settingsAfterReset = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterReset, Does.Contain("Left: Ignore"));
			Assert.That(settingsAfterReset, Does.Contain("Top: Ignore"));
			Assert.That(settingsAfterReset, Does.Contain("Right: Ignore"));
			Assert.That(settingsAfterReset, Does.Contain("Bottom: Ignore"));
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaResetNoneButtonFunctionality()
		{
			App.WaitForElement("ResetNoneButton");
			App.WaitForElement("CurrentSettings");

			// First set some checkboxes to checked state
			App.Tap("TopCheckBox");
			App.Tap("LeftCheckBox");

			// Tap Reset None button
			App.Tap("ResetNoneButton");

			// Verify all checkboxes are unchecked
			Assert.That(App.FindElement("TopCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("LeftCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("RightCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));
			Assert.That(App.FindElement("BottomCheckBox").GetAttribute<string>("checked"), Is.EqualTo("false"));

			// Verify settings label shows all "Respect"
			var settingsAfterReset = App.FindElement("CurrentSettings").GetText();
			Assert.That(settingsAfterReset, Does.Contain("Left: Respect"));
			Assert.That(settingsAfterReset, Does.Contain("Top: Respect"));
			Assert.That(settingsAfterReset, Does.Contain("Right: Respect"));
			Assert.That(settingsAfterReset, Does.Contain("Bottom: Respect"));
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaSyntaxOptimizationTest()
		{
			App.WaitForElement("CurrentSettings");

			// Test 1-value syntax (all edges same) - all ignore
			App.Tap("ResetAllButton");
			var allIgnoreSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allIgnoreSettings, Does.Contain("1-value syntax"));

			// Test 1-value syntax (all edges same) - all respect
			App.Tap("ResetNoneButton");
			var allRespectSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allRespectSettings, Does.Contain("1-value syntax"));

			// Test 2-value syntax (left/right same, top/bottom same)
			App.Tap("LeftCheckBox");
			App.Tap("RightCheckBox");
			var twoValueSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(twoValueSettings, Does.Contain("2-value syntax"));

			// Test 4-value syntax (all different)
			App.Tap("TopCheckBox");
			var fourValueSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(fourValueSettings, Does.Contain("4-value syntax"));
		}
	}
}