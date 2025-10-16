#if WINDOWS || MACCATALYST // TitleBar is only supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22058 : _IssuesUITest
{
	public override string Issue => "[Windows] OS system components ignore app theme";

	public Issue22058(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.TitleView)]
	public async Task VerifyTitleBarBackgroundColorChange()
	{
		try
		{
			App.WaitForElement("ThemeChangeButton");
			App.Tap("ThemeChangeButton");
			await Task.Delay(1000); // Slight delay to apply the theme change
			VerifyScreenshot(includeTitleBar: true);
		}
		finally
		{
			App.Tap("ResetThemeButton");
		}
	}

	[Test, Order(2)]
	[Category(UITestCategories.TitleView)]
	public async Task VerifyTimePickerTheme()
	{
		try
		{
			App.WaitForElement("ThemeChangeButton");
			App.Tap("ThemeChangeButton");
			await Task.Delay(1000); // Slight delay to apply the theme change

#if WINDOWS // TimePicker pop up is only supported on Windows
			App.Tap("TimePickerControl");
#endif
			VerifyScreenshot(includeTitleBar: true);
		}
		finally
		{
			App.TapCoordinates(50, 50);
			App.Tap("ResetThemeButton");
		}
	}
}
#endif