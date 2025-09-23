#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_ContentView : _IssuesUITest
{
	public override string Issue => "Test SafeArea ContentView for per-edge safe area control";

	public Issue28986_ContentView(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaContentViewBasicFunctionality()
	{
		var contentViewContent = App.WaitForElement("ContentViewContent");

		// 1. Ensure SafeAreaEdges is Default
		var initialSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(initialSettings, Does.Contain("Left: Default, Top: Default, Right: Default, Bottom: Default"));
		var contentViewWithDefaultSettings = contentViewContent.GetRect();
		// Verify that ContentView content starts at Y=0 when SafeAreaEdges is Default

		Assert.That(contentViewWithDefaultSettings.Y, Is.EqualTo(0),
		"ContentView should start at Y=0 when SafeAreaEdges is set to Default");		

		// 2. Ensure SafeAreaEdges are set to All
		App.Tap("ResetAllButton");
		var allSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(allSettings, Does.Contain("Left: All, Top: All, Right: All, Bottom: All"));
		var contentViewSafeAreaEdgesAll = App.WaitForElement("ContentViewContent").GetRect();
		Assert.That(contentViewSafeAreaEdgesAll.Y, Is.GreaterThanOrEqualTo(contentViewWithDefaultSettings.Y),
			"ContentView should move down (greater Y position) when SafeAreaEdges changes from Default to All");

		// 3. Ensure SafeAreaEdges are set to None (edge-to-edge)
		App.Tap("ResetNoneButton");
		var noneSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(noneSettings, Does.Contain("Left: None, Top: None, Right: None, Bottom: None"));
		var contentViewSafeAreaEdgesNone = App.WaitForElement("ContentViewContent").GetRect();

		// Verify that ContentView position is at Y=0 when SafeAreaEdges is None
		Assert.That(contentViewSafeAreaEdgesNone.Y, Is.EqualTo(0),
			"ContentView should be at Y=0 when SafeAreaEdges is set to None (edge-to-edge)");
	}
}
#endif