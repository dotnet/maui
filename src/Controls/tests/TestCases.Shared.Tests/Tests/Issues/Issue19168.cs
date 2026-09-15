using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19168 : _IssuesUITest
{
	public Issue19168(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "iOS Picker dismiss does not work when clicking outside of the Picker";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerShouldDismissWhenClickOnOutside()
	{
		App.WaitForElement("Picker");
		App.Tap("Picker");
#if MACCATALYST
		var pickerRect = App.WaitForElement("Picker").GetRect();
		App.TapCoordinates(pickerRect.X + 1, pickerRect.Y + 1);
		App.WaitForElement("Button");
#elif ANDROID
		App.WaitForElement("Baboon");
		App.TapCoordinates(0, 100);
#elif WINDOWS
		App.TapCoordinates(60, 600);
#elif IOS
		App.Tap("Button");
#endif
		VerifyScreenshot();
	}
}
