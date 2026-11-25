#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40498 : _IssuesUITest
{
	public Issue40498(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Overflow menu icons for Secondary toolbar items are blurry because GetPlatformMenuImage renders FontImageSource at 1x";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void OverflowMenuIconsAreRenderedAtScreenScale()
	{
		App.WaitForElement("Issue40498_DescriptionLabel");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Info");
		VerifyScreenshot();
	}
}
#endif
