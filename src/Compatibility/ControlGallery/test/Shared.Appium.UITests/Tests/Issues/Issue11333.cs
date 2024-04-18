#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11333 : IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue11333(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] SwipeView does not work on Android if child has TapGestureRecognizer";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[FailsOnAndroid]
		public void SwipeWithChildGestureRecognizer()
		{
			RunningApp.WaitForElement(SwipeViewId);
			RunningApp.SwipeRightToLeft();
			RunningApp.Tap(SwipeViewId);
			RunningApp.WaitForElement("ResultLabel");
		}
	}
}
#endif