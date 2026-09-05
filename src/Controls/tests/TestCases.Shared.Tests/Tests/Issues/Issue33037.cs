#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33037 : _IssuesUITest
{
	public Issue33037(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Large Title display disappears when scrolling in Shell";

	[Test]
	[Category(UITestCategories.Shell)]
	public void LargeTitleTransitionsOnScroll()
	{
		// Verify page loaded with large title visible
		App.WaitForElement("PageTitle");

		// Take a screenshot before scrolling to verify large title is shown
		VerifyScreenshot("Issue33037_BeforeScroll");

		// Scroll down to trigger the large title → standard title transition
		App.ScrollDown("TestScrollView");
		App.ScrollDown("TestScrollView");

		// Take screenshot after scrolling — title should have transitioned to standard size
		VerifyScreenshot("Issue33037_AfterScroll", retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
