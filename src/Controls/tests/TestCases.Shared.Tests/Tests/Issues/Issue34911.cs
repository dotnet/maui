#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34911 : _IssuesUITest
{
	public Issue34911(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Android] StatusBar color doesn't update immediately when changing UserAppTheme in Shell";

	[Test]
	[Category(UITestCategories.Shell)]
	public void StatusBarColorShouldUpdateImmediatelyOnThemeChange()
	{
		// Start in Light mode
		App.WaitForElement("ToggleThemeButton");
		App.Screenshot("LightTheme");

		// Switch to Dark mode: StatusBar should immediately update
		App.Tap("ToggleThemeButton");
		App.WaitForElement("ToggleThemeButton");
		App.Screenshot("DarkTheme");

		// Toggle back to Light mode to confirm it's not stuck
		App.Tap("ToggleThemeButton");
		App.WaitForElement("ToggleThemeButton");
		App.Screenshot("BackToLightTheme");
	}
}
#endif
