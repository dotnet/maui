using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueSafeAreaAndroidNavBar : _IssuesUITest
	{
		public IssueSafeAreaAndroidNavBar(TestDevice device) : base(device) { }

		public override string Issue => "Test content flowing behind navigation bar on Android when SafeAreaEdges=None";

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void ContentFlowsBehindNavigationBarWithSafeAreaNone()
		{
			// Start with SafeAreaEdges.None (default in XAML)
			App.WaitForElement("HeaderLabel");

			// Verify the current safe area setting
			var currentSafeAreaLabel = App.WaitForElement("CurrentSafeArea").GetText();
			Assert.That(currentSafeAreaLabel, Does.Contain("None"));

			// Click button to set SafeAreaEdges.All
			App.Tap("SetSafeAreaAllButton");
			App.WaitForElement("TestStatus");

			// Verify safe area changed to All
			var updatedSafeAreaLabel = App.WaitForElement("CurrentSafeArea").GetText();
			Assert.That(updatedSafeAreaLabel, Does.Contain("All"));

			// Click button to set SafeAreaEdges.None again
			App.Tap("SetSafeAreaNoneButton");
			App.WaitForElement("TestStatus");

			// Verify safe area changed back to None
			var finalSafeAreaLabel = App.WaitForElement("CurrentSafeArea").GetText();
			Assert.That(finalSafeAreaLabel, Does.Contain("None"));
		}

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void TopNoneBottomAllConfiguration()
		{
			App.WaitForElement("HeaderLabel");

			// Click button to set Top=None, Bottom=All
			App.Tap("SetTopNoneBottomAllButton");
			App.WaitForElement("TestStatus");

			// Verify safe area setting
			var safeAreaLabel = App.WaitForElement("CurrentSafeArea").GetText();
			// Top should be None
			Assert.That(safeAreaLabel, Does.Contain("T=None"));
			// Bottom should be All
			Assert.That(safeAreaLabel, Does.Contain("B=All"));
		}
	}
}
