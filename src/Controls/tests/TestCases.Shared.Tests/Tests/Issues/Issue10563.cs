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
			
			// Test Left SwipeItems
			App.WaitForElement(OpenLeftId);
			App.Tap(OpenLeftId);
			// Wait for swipe animation to complete - the SwipeItem text becomes visible
			App.WaitForElement("Issue 10563");
			VerifyScreenshotOrSetException(ref exception, "Left_SwipeItems", tolerance: 4.0);
			App.Tap(CloseId);
			// Wait for close animation to complete - the SwipeItem text disappears
			App.WaitForNoElement("Issue 10563");

			// Test Right SwipeItems
			App.WaitForElement(OpenRightId);
			App.Tap(OpenRightId);
			App.WaitForElement("Issue 10563");
			VerifyScreenshotOrSetException(ref exception, "Right_SwipeItems", tolerance: 4.0);
			App.Tap(CloseId);
			App.WaitForNoElement("Issue 10563");

			// Test Top SwipeItems
			App.WaitForElement(OpenTopId);
			App.Tap(OpenTopId);
			App.WaitForElement("Issue 10563");
			VerifyScreenshotOrSetException(ref exception, "Top_SwipeItems", tolerance: 4.0);
			App.Tap(CloseId);
			App.WaitForNoElement("Issue 10563");

			// Test Bottom SwipeItems
			App.WaitForElement(OpenBottomId);
			App.Tap(OpenBottomId);
			App.WaitForElement("Issue 10563");
			VerifyScreenshotOrSetException(ref exception, "Bottom_SwipeItems", tolerance: 4.0);
			App.Tap(CloseId);

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
#endif