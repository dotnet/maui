#if ANDROID // Edge-to-edge system bar behavior (transparency, nav scrim, icon theming) is Android-specific.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32987 : _IssuesUITest
{
	public override string Issue => "Android edge-to-edge: system bars are transparent, the 3-button navigation scrim is removed, and status bar icons follow the app theme";

	public Issue32987(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SystemBarsAreTransparentAndNavScrimRemoved()
	{
		App.WaitForElement("BarStateReady");

		Assert.That(App.WaitForElement("BarTransparentValue").GetText(), Is.EqualTo("True"),
			"Status and navigation bars should be transparent under edge-to-edge.");
		Assert.That(App.WaitForElement("NavScrimValue").GetText(), Is.EqualTo("False"),
			"The 3-button navigation bar contrast scrim should be removed under edge-to-edge.");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void StatusBarIconsFollowThemeChange()
	{
		App.WaitForElement("BarStateReady");

		try
		{
			// The theme markers are set in the same pass that reads the bar icons, so the icon value
			// is settled once the marker appears.
			App.SetLightTheme();
			App.WaitForElement("ThemeIsLight");
			Assert.That(App.WaitForElement("LightIconsValue").GetText(), Is.EqualTo("True"),
				"Light theme should use dark (light-appearance) status bar icons.");

			App.SetDarkTheme();
			App.WaitForElement("ThemeIsDark");
			Assert.That(App.WaitForElement("LightIconsValue").GetText(), Is.EqualTo("False"),
				"Dark theme should use light status bar icons.");
		}
		finally
		{
			App.SetLightTheme();
		}
	}
}
#endif
