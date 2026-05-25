using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23074(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "SwipeItem IconImageSource should allow more configuration";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemFontAndSvgIconsRenderCorrectly()
	{
		App.WaitForElement("SwipeContent");
		App.SwipeRightToLeft("SwipeViewWithIcons");
		App.WaitForElement("FontSwipeItem");
		VerifyScreenshot();
	}
}
