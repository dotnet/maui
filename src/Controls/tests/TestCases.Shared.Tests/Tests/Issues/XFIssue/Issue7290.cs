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

	//[Test]
	//[Category(UITestCategories.Shell)]
	//[FailsOnAndroid]
	//[FailsOnIOS]
	//public void DisplayActionSheetAndDisplayAlertFromOnAppearing()
	//{
	//	App.Tap("Cancel");
	//	App.Tap("Close Action Sheet");
	//}
}