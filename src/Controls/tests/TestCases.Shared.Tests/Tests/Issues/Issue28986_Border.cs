#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_Border : _IssuesUITest
{
	public override string Issue => "Test SafeArea Border for per-edge safe area control";

	public Issue28986_Border(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaBorderBasicFunctionality()
	{
		var borderContent = App.WaitForElement("BorderContent");

		// 1. Ensure SafeAreaEdges is Default
		var initialSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(initialSettings, Does.Contain("Left: Default, Top: Default, Right: Default, Bottom: Default"));
		var borderWithDefaultSettings = borderContent.GetRect();
		// Verify that Border starts at Y=5 when SafeAreaEdges is Default
		// Because the border stroke thickness is 5
		Assert.That(borderWithDefaultSettings.Y, Is.EqualTo(5), "Border should start at Y=5 when SafeAreaEdges is set to Default");

		// 2. Ensure SafeAreaEdges are set to All
		App.Tap("ResetAllButton");
		var allSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(allSettings, Does.Contain("Left: All, Top: All, Right: All, Bottom: All"));
		var borderSafeAreaEdgesAll = App.WaitForElement("BorderContent").GetRect();
		Assert.That(borderSafeAreaEdgesAll.Y, Is.GreaterThan(borderWithDefaultSettings.Y), "Border should move down (greater Y position) when SafeAreaEdges changes from Default to All");

		// 3. Ensure SafeAreaEdges are set to None (edge-to-edge)
		App.Tap("ResetNoneButton");
		var noneSettings = App.FindElement("CurrentSettings").GetText();
		Assert.That(noneSettings, Does.Contain("Left: None, Top: None, Right: None, Bottom: None"));
		var borderSafeAreaEdgesNone = App.WaitForElement("BorderContent").GetRect();
		// Verify that Border position is at Y=5 when SafeAreaEdges is None
		// This is because the border stroke thickness is 5
		Assert.That(borderSafeAreaEdgesNone.Y, Is.EqualTo(5),
			"Border should be at Y=5 when SafeAreaEdges is set to None (edge-to-edge)");
	}
}
#endif