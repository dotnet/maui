#if TEST_FAILS_ON_CATALYST //https://github.com/dotnet/maui/issues/20904
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27803 : _IssuesUITest
{
	public Issue27803(TestDevice device) : base(device)
	{
	}

	public override string Issue => "DatePicker default format on iOS";

	[Fact]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerTextColorShouldUpdate()
	{
		App.WaitForElement("MauiButton");

		App.Tap("MauiButton");

		VerifyScreenshot();
	}
}
#endif