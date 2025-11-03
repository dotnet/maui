using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32287 : _IssuesUITest
{
	public Issue32287(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Using custom TitleView in AppShell causes shell content to be covered in iOS 26";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomTitleViewDoesNotCoverContent()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomTitleViewDoesNotCoverContentInLandscape()
	{
		App.SetOrientationLandscape();
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomTitleViewTracksContentSizeChanges()
	{
		App.WaitForElement("Label");
		
		// Increase font size and verify TitleView still doesn't cover content
		App.Tap("IncreaseFontSizeButton");
		App.WaitForElement("Label");
		VerifyScreenshot();
		
		// Increase again
		App.Tap("IncreaseFontSizeButton");
		App.WaitForElement("Label");
		VerifyScreenshot();
		
		// Decrease font size
		App.Tap("DecreaseFontSizeButton");
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
#endif
}
