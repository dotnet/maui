using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33783 : _IssuesUITest
{
	public Issue33783(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Switch ThumbColor not applied correctly when theme changes on iOS";

	[Test, Order(0)]
	[Category(UITestCategories.Switch)]
	public async Task VerifySwitchThumbColorOnDarkThemeChange()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		await Task.Delay(500);
		VerifyScreenshot("Issue33783_SwitchThumbColor_DarkTheme");
	}

	[Test, Order(1)]
	[Category(UITestCategories.Switch)]
	public async Task VerifySwitchThumbColorOnLightThemeChange()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		await Task.Delay(500);
		VerifyScreenshot("Issue33783_SwitchThumbColor_LightTheme");
	}
}