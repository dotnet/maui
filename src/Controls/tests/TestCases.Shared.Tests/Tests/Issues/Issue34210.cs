#if TEST_FAILS_ON_WINDOWS // Cannot open programmatically a SwipeView on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34210 : _IssuesUITest
{
	public override string Issue => "SwipeItem ignores FontImageSource rendered size on Android";

	public Issue34210(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemFontImageSourceSizeIsRespected()
	{
		App.WaitForElement("OpenSwipeViewButton");
		App.Tap("OpenSwipeViewButton");
		App.WaitForElement("Action");
		VerifyScreenshot();
	}
}
#endif