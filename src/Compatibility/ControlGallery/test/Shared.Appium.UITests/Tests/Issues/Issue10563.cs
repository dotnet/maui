#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    public class Issue10563 : IssuesUITest
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
		[FailsOnIOS]
		public void Issue10563OpenSwipeViewTest()
		{
			RunningApp.WaitForElement(OpenLeftId);
			RunningApp.Tap(OpenLeftId);
			RunningApp.Screenshot("Left SwipeItems");
			RunningApp.Tap(CloseId);

			RunningApp.WaitForElement(OpenRightId);
			RunningApp.Tap(OpenRightId);
			RunningApp.Screenshot("Right SwipeItems");

			RunningApp.Tap(CloseId);

			RunningApp.WaitForElement(OpenTopId);
			RunningApp.Tap(OpenTopId);
			RunningApp.Screenshot("Top SwipeItems");
			RunningApp.Tap(CloseId);

			RunningApp.WaitForElement(OpenBottomId);
			RunningApp.Tap(OpenBottomId);
			RunningApp.Screenshot("Bottom SwipeItems");
			RunningApp.Tap(CloseId);
		}
	}
}
#endif