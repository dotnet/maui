#if WINDOWS || ANDROID // The back hardware button behavior is only applicable for Windows and Android. On iOS and macOS, the picker closes only when the Done button is clicked.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7311 : _IssuesUITest
{
	const string FirstPickerItem = "Uno";
	const string PickerId = "CaptainPickard";

	public Issue7311(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [Android] Error back hardware button with Picker";

	[Test]
	[Category(UITestCategories.Picker)]
	public void OpeningPickerPressingBackButtonTwiceShouldNotOpenPickerAgain()
	{
		App.WaitForElement(PickerId);
		App.Tap(PickerId);

		App.WaitForElement(FirstPickerItem);

		App.Back();

		App.WaitForNoElement(FirstPickerItem);

		//The Below actions are not possible due to the current implementation of the Host app, the issue page has designated the MainPage of the Current Application. 
		//App.Back();
		//App.WaitForNoElement(FirstPickerItem, "Picker is again visible after second back button press", TimeSpan.FromSeconds(10));
	}
}
#endif