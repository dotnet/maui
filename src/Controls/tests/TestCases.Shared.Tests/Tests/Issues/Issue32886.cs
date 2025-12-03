using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32886 : _IssuesUITest
{
	public Issue32886(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android, iOS, Mac] Entry ClearButton not visible on dark theme";

	[Test, Order(1)]
	[Category(UITestCategories.Entry)]
	public void EntryClearButtonShouldBeVisibleOnLightTheme()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
#if ANDROID // On Android, to address CI flakiness, the keyboard is dismissed.
		if (App.WaitForKeyboardToShow(timeout: TimeSpan.FromSeconds(1)))
		{
			App.DismissKeyboard();
		}
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1550);
#else
		VerifyScreenshot();
#endif
	}

	[Test, Order(2)]
	[Category(UITestCategories.Entry)]
	public void EntryClearButtonShouldBeVisibleOnDarkTheme()
	{
		App.WaitForElement("TestEntry");
		App.Tap("ThemeButton");
#if WINDOWS // On Windows, the clear button isn't visible when Entry loses focus, so manually focused to check its icon color.
        App.Tap("TestEntry");
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1550);
#else
		VerifyScreenshot();
#endif
	}
}