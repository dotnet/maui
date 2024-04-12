#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11969 : IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeButtonId = "SwipeButtonId";

		const string Failed = "SwipeView Button not tapped";
		const string Success = "SUCCESS";

		public Issue11969(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Disabling Swipe view not handling tap gesture events on the content in iOS of Xamarin Forms";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[FailsOnIOS]
		public void SwipeDisableChildButtonTest()
		{
			RunningApp.WaitForNoElement(Failed);
			RunningApp.WaitForElement(SwipeViewId);
			RunningApp.Tap("SwipeViewCheckBoxId");
			RunningApp.Tap("SwipeViewContentCheckBoxId");
			RunningApp.Tap(SwipeButtonId);
			RunningApp.WaitForNoElement(Success);
		}
	}
}
#endif