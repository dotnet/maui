using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15695 : _IssuesUITest
{
	public override string Issue => "Support for Switch OFF State color";

	public Issue15695(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Switch)]
	public async Task VerifySwitchOffColorAfterToggling()
	{
		App.WaitForElement("Switch");
		App.Tap("ResetStateButton");
		App.Tap("ToggleSwitch");
		// Wait for the switch to animate
		await Task.Delay(500);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Switch)]
	public async Task VerifySwitchOffColorAfterTogglingDarkTheme()
	{
		App.WaitForElement("Switch");
		App.Tap("ResetStateButton");
		App.Tap("ChangeThemeButton");
		App.Tap("ToggleSwitch");
		// Wait for the switch to animate
		await Task.Delay(500);
		VerifyScreenshot();
	}


	[Test]
	[Category(UITestCategories.Switch)]
	public async Task VerifySwitchNullColor()
	{
		App.WaitForElement("Switch");
		App.Tap("ResetStateButton");
		App.Tap("NullOffColorButton");
		App.Tap("ToggleSwitch");
		// Wait for the switch to animate
		await Task.Delay(500);
		VerifyScreenshot();
	}
}