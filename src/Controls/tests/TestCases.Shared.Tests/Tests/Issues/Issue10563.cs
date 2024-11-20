#if ANDROID || IOS
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue10563OpenSwipeViewTest()
		{
			App.WaitForElement(OpenLeftId);
			App.Tap(OpenLeftId);
			App.Screenshot("Left SwipeItems");
			App.Tap(CloseId);

			App.WaitForElement(OpenRightId);
			App.Tap(OpenRightId);
			App.Screenshot("Right SwipeItems");

			App.Tap(CloseId);

			App.WaitForElement(OpenTopId);
			App.Tap(OpenTopId);
			App.Screenshot("Top SwipeItems");
			App.Tap(CloseId);

			App.WaitForElement(OpenBottomId);
			App.Tap(OpenBottomId);
			App.Screenshot("Bottom SwipeItems");
			App.Tap(CloseId);
		}
	}
}
#endif