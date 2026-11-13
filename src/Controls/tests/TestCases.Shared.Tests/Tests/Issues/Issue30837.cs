using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30837 : _IssuesUITest
{
	public override string Issue => "[iOS] TimePicker AM/PM frequently changes when the app is closed and reopened";

	public Issue30837(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void VerifyTimePickerFormat()
	{
		App.WaitForElement("TimePickerLabel");
		VerifyScreenshot();
	}
}