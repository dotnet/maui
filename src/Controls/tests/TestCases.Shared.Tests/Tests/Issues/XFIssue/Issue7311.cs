using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7311 : _IssuesUITest
{
	public Issue7311(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [Android] Error back hardware button with Picker";

	//[Test]
	//[Category(UITestCategories.Picker)]
	//public void OpeningPickerPressingBackButtonTwiceShouldNotOpenPickerAgain()
	//{
	//	RunningApp.WaitForElement(PickerId);
	//	RunningApp.Tap(PickerId);

	//	RunningApp.WaitForElement(FirstPickerItem);

	//	RunningApp.Back();

	//	RunningApp.WaitForNoElement(FirstPickerItem);

	//	RunningApp.Back();

	//	RunningApp.WaitForNoElement(FirstPickerItem, "Picker is again visible after second back button press", TimeSpan.FromSeconds(10));

	//	RunningApp.Screenshot("Back at the previous page, not showing the picker again");
	//}
}