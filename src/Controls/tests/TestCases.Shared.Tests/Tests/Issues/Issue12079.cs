#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //This test case verifies "SwipeLeftToRight method" exclusively on the Android and IOS platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12079 : _IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue12079(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeView crash if Text not is set on SwipeItem";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		public void SwipeItemNoTextWindows()
		{
			App.WaitForElement(SwipeViewId);
			App.SwipeLeftToRight(SwipeViewId);
			App.Tap(SwipeViewId);
			App.WaitForElement("Success");
			VerifyScreenshot();
		}
	}
}
#endif