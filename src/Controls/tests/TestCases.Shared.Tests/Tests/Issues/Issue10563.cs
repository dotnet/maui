#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //SwipeView does not work correctly when opened programmatically, for more information: https://github.com/dotnet/maui/issues/17204, https://github.com/dotnet/maui/issues/22153 & https://github.com/dotnet/maui/issues/14777
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10563 : _IssuesUITest
	{
		const string OpenLeftId = "OpenLeftId";
		const string OpenRightId = "OpenRightId";
		const string OpenTopId = "OpenTopId";
		const string OpenBottomId = "OpenBottomId";
		const string CloseId = "CloseId";

		public Issue10563(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] SwipeView Open methods does not work for RightItems";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue10563OpenSwipeViewTest()
		{
			Exception? exception = null;
			App.WaitForElement(OpenLeftId);
			App.Tap(OpenLeftId);
			VerifyScreenshotOrSetException(ref exception, "Left_SwipeItems");
			App.Tap(CloseId);

			App.WaitForElement(OpenRightId);
			App.Tap(OpenRightId);
			VerifyScreenshotOrSetException(ref exception, "Right_SwipeItems");
			App.Tap(CloseId);

			App.WaitForElement(OpenTopId);
			App.Tap(OpenTopId);
			VerifyScreenshotOrSetException(ref exception, "Top_SwipeItems");
			App.Tap(CloseId);

			App.WaitForElement(OpenBottomId);
			App.Tap(OpenBottomId);
			VerifyScreenshotOrSetException(ref exception, "Bottom_SwipeItems");
			App.Tap(CloseId);

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
#endif