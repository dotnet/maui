#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_ScrollView : _IssuesUITest
{
	public override string Issue => "Test SafeArea ScrollView for per-edge safe area control";

	public Issue28986_ScrollView(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaScrollViewBasicFunctionality()
	{
		// 1. Test loads - verify essential elements are present
		App.WaitForElement("TestScrollView");

		// 2. Verify initial state - ScrollView should start with All (full safe area)
		var initialSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(initialSettings, Does.Contain("All (Full safe area)"));

		// 3. Get initial ScrollView position when SafeAreaEdges is All
		var scrollViewWithSafeArea = App.WaitForElement("ScrollViewContent").GetRect();

		// 4. Set ScrollView SafeAreaEdges to None (edge-to-edge)
		App.Tap("ScrollViewResetNoneButton");
		var noneSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(noneSettings, Does.Contain("None (Edge-to-edge)"));

		// 5. Get ScrollView position after setting to None
		var scrollViewEdgeToEdge = App.WaitForElement("ScrollViewContent").GetRect();

		// 6. Verify that ScrollView position/size changes when SafeAreaEdges changes
		// When ScrollView has SafeAreaEdges.All, it respects safe area insets
		// When ScrollView has SafeAreaEdges.None, it goes edge-to-edge
		Assert.That(scrollViewEdgeToEdge.Y, Is.LessThanOrEqualTo(scrollViewWithSafeArea.Y),
			"ScrollView should move up (smaller Y position) when SafeAreaEdges changes from All to None");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void SafeAreaScrollViewContainerFunctionality()
	{
		App.WaitForElement("TestScrollView");

		// Start with None to establish baseline
		App.Tap("ScrollViewResetNoneButton");
		var nonePosition = App.WaitForElement("ScrollViewContent").GetRect();

		// Test Container button functionality
		App.Tap("ScrollViewSetContainerButton");
		var containerSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(containerSettings, Does.Contain("Container (Respect notches/bars)"));

		// When you change from none back to container
		// the scrollview will just stay scrolled up at this point
		// it doesn't reset to zero
		App.ScrollUp("CurrentSettings");

		var containerPosition = App.WaitForElement("ScrollViewContent").GetRect();
		Assert.That(containerPosition.Y, Is.GreaterThanOrEqualTo(nonePosition.Y),
			"ScrollView should move down when SafeAreaEdges changes from None to Container");
	}
}
#endif