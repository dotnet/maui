#if ANDROID || IOS  // SafeAreaEdges not supported on Catalyst and Windows

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33034 : _IssuesUITest
{
	public override string Issue => "SafeAreaEdges works correctly only on the first tab in Shell. Other tabs have content colliding with the display cutout in the landscape mode.";

	public Issue33034(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaShouldWorkOnAllShellTabs()
	{
		App.WaitForElement("EdgeLabel");
		App.SetOrientationLandscape();
		var initialRect = App.WaitForElement("EdgeLabel").GetRect();

		App.TapTab("Second Tab");
		App.WaitForElement("EdgeLabel");
		App.TapTab("First Tab");
		var afterSwitchRect = App.WaitForElement("EdgeLabel").GetRect();

		Assert.That(afterSwitchRect.X, Is.EqualTo(initialRect.X).Within(5));
		Assert.That(afterSwitchRect.Width, Is.EqualTo(initialRect.Width).Within(5));
	}
}
#endif
