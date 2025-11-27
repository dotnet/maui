using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32886 : _IssuesUITest
{
	public Issue32886(TestDevice device) : base(device)
	{
	}

#if IOS
	private const int CropBottomValue = 1550;
#elif ANDROID
	private const int CropBottomValue = 1150;
#elif WINDOWS
	private const int CropBottomValue = 400;
#else
	private const int CropBottomValue = 360;
#endif

	public override string Issue => "[Android, iOS, Mac] Entry ClearButton not visible on dark theme";

	[Test, Order(1)]
	[Category(UITestCategories.Entry)]
	public void EntryClearButtonShouldBeVisibleOnLightTheme()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(2)]
	[Category(UITestCategories.Entry)]
	public void EntryClearButtonShouldBeVisibleOnDarkTheme()
	{
		App.WaitForElement("TestEntry");
		App.Tap("ThemeButton");
		App.Tap("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
}