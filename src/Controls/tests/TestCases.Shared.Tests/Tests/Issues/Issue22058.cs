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

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void VerifyTimePickerTheme()
	{
		App.WaitForElement("ThemeChangeButton");
		App.Tap("ThemeChangeButton");

#if WINDOWS //TimePicker pop up is only supported on Windows
		App.Tap("TimePickerControl");
		Thread.Sleep(500); // Slight delay to allow the TimePicker to open
#endif
		VerifyScreenshot(includeTitleBar: true);
		App.TapCoordinates(50, 50);
		App.Tap("ThemeChangeButton");
	}
}
#endif