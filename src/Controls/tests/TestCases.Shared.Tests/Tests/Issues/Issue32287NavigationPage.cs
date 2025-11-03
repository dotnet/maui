using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32287NavigationPage : _IssuesUITest
{
	public Issue32287NavigationPage(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Using custom TitleView in NavigationPage causes content to be covered in iOS 26";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void CustomTitleViewDoesNotCoverContent()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void CustomTitleViewTracksContentSizeChanges()
	{
		App.WaitForElement("Label");
		
		// Increase font size and verify TitleView still doesn't cover content
		App.Tap("IncreaseFontSizeButton");
		App.Tap("IncreaseFontSizeButton");
		App.Tap("IncreaseFontSizeButton");
		App.WaitForElement("Label");
		VerifyScreenshot();
	}

#if ANDROID && IOS
	[Test]
	[Category(UITestCategories.Navigation)]
	public void CustomTitleViewDoesNotCoverContentInLandscape()
	{
		App.SetOrientationLandscape();
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
#endif
}
