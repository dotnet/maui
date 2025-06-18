#if TEST_FAILS_ON_CATALYST //https://github.com/dotnet/maui/issues/20904
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27803 : _IssuesUITest
{
	public Issue27803(TestDevice device) : base(device)
	{
	}

	public override string Issue => "DatePicker default format on iOS";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerTextColorShouldUpdate()
	{
		App.WaitForElement("MauiButton");

		App.Tap("MauiButton");

		VerifyScreenshot();
	}
}
#endif