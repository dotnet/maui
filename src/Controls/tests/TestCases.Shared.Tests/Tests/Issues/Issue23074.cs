#if TEST_FAILS_ON_WINDOWS //The AutomationId for SwipeView items does not function as expected on the Windows platform. Additionally, programmatic swiping is currently not working. For reference:  https://github.com/dotnet/maui/issues/14777.
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
		VerifyScreenshot();
	}
}
#endif