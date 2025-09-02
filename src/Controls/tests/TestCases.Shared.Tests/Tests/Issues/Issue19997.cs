using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19997 : _IssuesUITest
{
	public Issue19997(TestDevice device) : base(device)
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
	public override string Issue => "[Android, iOS, MacOS] Entry ClearButton Color Not Updating on AppThemeBinding Change";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryClearButtonColorShouldUpdateOnThemeChange()
	{
		App.WaitForElement("EntryWithAppThemeBinding");
		App.Tap("EntryWithAppThemeBinding");
		App.Tap("ThemeButton");
#if WINDOWS // On Windows, the clear button isn't visible when Entry loses focus, so manually focused to check its icon color.
            App.Tap("EntryWithAppThemeBinding");
#endif
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
}
