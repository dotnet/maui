// This regression is Android-only because it exercises WebView.HitTestResult URL resolution for image anchors.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35157(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "Target blank link with data URI image crashes BlazorWebView";

	[Test]
	[Category(UITestCategories.WebView)]
	public void TargetBlankLinkWithDataImageDoesNotCrash()
	{
		var imageLink = App.WaitForElement(
			AppiumQuery.ByXPath("//*[@content-desc='Open Google' and @clickable='true']"));
		imageLink.Tap();

		// Confirm the tap actually triggered external navigation (not just a silent no-op):
		// the survival label should momentarily disappear from view as the external browser
		// takes the foreground. If a future regression stops opening the resolved anchor
		// link, the label would remain visible and this assertion would catch it.
		App.WaitForNoElement("Issue35157SurvivalLabel", "External browser did not open for the image anchor link");

		App.ForegroundApp();
		App.WaitForElement("Issue35157SurvivalLabel");
	}
}
#endif
