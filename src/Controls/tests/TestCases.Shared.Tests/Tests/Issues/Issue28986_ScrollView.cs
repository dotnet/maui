#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28986_ScrollView : _IssuesUITest
	{
		public override string Issue => "Test SafeArea ScrollView for per-edge safe area control";

		public Issue28986_ScrollView(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void SafeAreaScrollViewBasicFunctionality()
		{
			// 1. Test loads - verify essential elements are present
			App.WaitForElement("TestScrollView");

			// 2. Verify initial state - ScrollView should start with All (full safe area)
			var initialSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(initialSettings, Does.Contain("All (Full safe area)"));

			// 3. Test None button functionality
			App.Tap("ScrollViewResetNoneButton");
			var noneSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));

			// 4. Test All button functionality
			App.Tap("ScrollViewResetAllButton");
			var allSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allSettings, Does.Contain("All (Full safe area)"));
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void SafeAreaScrollViewContainerFunctionality()
		{
			App.WaitForElement("TestScrollView");
			App.WaitForElement("CurrentSettings");

			// Test Container button functionality
			App.Tap("ScrollViewSetContainerButton");
			var containerSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));

			// Test SoftInput button functionality
			App.Tap("ScrollViewSetSoftInputButton");
			var softInputSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(softInputSettings, Does.Contain("SoftInput (Avoid keyboard only)"));
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void SafeAreaScrollViewSequentialTesting()
		{
			App.WaitForElement("TestScrollView");
			App.WaitForElement("CurrentSettings");

			// Test sequence: All -> None -> Container -> SoftInput -> All
			
			// 1. Set to All
			App.Tap("ScrollViewResetAllButton");
			var allSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(allSettings, Does.Contain("All (Full safe area)"));

			// 2. Set to None
			App.Tap("ScrollViewResetNoneButton");
			var noneSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));

			// 3. Set to Container
			App.Tap("ScrollViewSetContainerButton");
			var containerSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));

			// 4. Set to SoftInput
			App.Tap("ScrollViewSetSoftInputButton");
			var softInputSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(softInputSettings, Does.Contain("SoftInput (Avoid keyboard only)"));

			// 5. Return to All
			App.Tap("ScrollViewResetAllButton");
			var finalAllSettings = App.FindElement("CurrentSettings").GetText();
			Assert.That(finalAllSettings, Does.Contain("All (Full safe area)"));
		}
	}
}
#endif