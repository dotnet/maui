#if WINDOWS
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
		App.Tap("timePicker");

		Thread.Sleep(500); // Slight delay to allow the TimePicker to open
		VerifyScreenshot();
		App.TapCoordinates(10, 10);
		App.Tap("ThemeChangeButton");
	}
}
#endif