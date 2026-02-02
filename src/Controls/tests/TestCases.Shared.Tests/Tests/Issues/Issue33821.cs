#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33821 : _IssuesUITest
{
	public Issue33821(TestDevice testDevice) : base(testDevice) { }

	private const string TestPickerId = "TestPicker";
	private const string DarkThemeButtonId = "DarkThemeButton";
	private const string LightThemeButtonId = "LightThemeButton";

	public override string Issue => "Picker does not respect themes (outline, dropdown, indicator)";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerDropdownShouldRespectThemeChange()
	{
		App.WaitForElement(TestPickerId);

		// Start with light theme
		App.WaitForElement(LightThemeButtonId);
		App.Tap(LightThemeButtonId);

		// Open the picker dropdown in light theme
		App.Tap(TestPickerId);
		App.WaitForElement(TestPickerId);
		// Give the dropdown time to appear
		Task.Delay(500).Wait();
		VerifyScreenshot("PickerDropdownLightTheme");

		// Close the dropdown by tapping outside or pressing escape
		App.TapCoordinates(10, 10);
		Task.Delay(300).Wait();

		// Switch to dark theme
		App.WaitForElement(DarkThemeButtonId);
		App.Tap(DarkThemeButtonId);

		// Open the picker dropdown in dark theme
		App.Tap(TestPickerId);
		App.WaitForElement(TestPickerId);
		// Give the dropdown time to appear
		Task.Delay(500).Wait();
		VerifyScreenshot("PickerDropdownDarkTheme");

		// Cleanup - close dropdown and reset to light theme
		App.TapCoordinates(10, 10);
		Task.Delay(300).Wait();
		App.Tap(LightThemeButtonId);
	}
}
#endif
