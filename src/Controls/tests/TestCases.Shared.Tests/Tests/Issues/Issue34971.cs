using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34971 : _IssuesUITest
{
	string doneButton => App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp) ? "selected" : "Done";

	public Issue34971(TestDevice device) : base(device)
    {
    }

	public override string Issue => "Picker CharacterSpacing lost after item selection when Title is set";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerKeepsCharacterSpacingAfterSelectingItem()
	{
		Exception? exception = null;

		App.WaitForElement("CharSpacingPicker");
		App.Tap("CharSpacingPicker");
		CloseOpenedPicker();
		App.WaitForElement("CharSpacingPicker");
		VerifyScreenshotOrSetException(ref exception, "PickerKeepsCharacterSpacingAfterSelectingItem");

		App.WaitForElement("SelectIndexButton");
		App.Tap("SelectIndexButton");
		App.WaitForElement("CharSpacingPicker");
		VerifyScreenshotOrSetException(ref exception, "PickerKeepsCharacterSpacingAfterProgrammaticSelectedIndexChange");

		if (exception is not null)
		{
			throw exception;
		}
	}

	// The picker is presented differently per platform, so it is dismissed differently:
	// iOS/MacCatalyst confirm the wheel with a Done button (which selects the highlighted item),
	// on iOS 26+ the button is labeled "selected" instead of "Done".
	// Android dismisses with Cancel, and Windows dismisses by tapping outside the dropdown.
	void CloseOpenedPicker()
	{
#if ANDROID
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
#elif IOS || MACCATALYST
		App.WaitForElement(doneButton);
		App.Tap(doneButton);
#elif WINDOWS
		App.TapCoordinates(10, 10);
#endif
	}
}