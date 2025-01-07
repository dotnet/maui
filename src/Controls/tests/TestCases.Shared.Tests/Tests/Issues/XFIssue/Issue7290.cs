#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // DisplayActionSheet and DisplayAlert are not working in initial loading on Android and Windows, Issue: https://github.com/dotnet/maui/issues/26481
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7290 : _IssuesUITest
{

	public Issue7290(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] DisplayActionSheet or DisplayAlert in OnAppearing does not work on Shell";

	[Test]
	[Category(UITestCategories.Shell)]
	public void DisplayActionSheetAndDisplayAlertFromOnAppearing()
	{
		App.TapDisplayAlertButton("Cancel");
		App.TapDisplayAlertButton("Close Action Sheet", buttonIndex: 1);
	}
}
#endif