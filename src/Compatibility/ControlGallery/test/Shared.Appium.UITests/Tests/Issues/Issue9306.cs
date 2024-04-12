using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue9306 : IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeItemId = "SwipeItemId";
		const string LeftCountLabelId = "LeftCountLabel";

		public Issue9306(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Cannot un-reveal swipe view items on iOS / Inconsistent swipe view behaviour";

		[Test]
		[Category(UITestCategories.SwipeView)]
		[FailsOnIOS]
		public void Issue9306SwipeViewCloseSwiping()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(SwipeViewId);

			RunningApp.SwipeLeftToRight(SwipeViewId);
			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.SwipeLeftToRight(SwipeViewId);

			RunningApp.Tap(SwipeItemId);

			var result = RunningApp.FindElement(LeftCountLabelId).GetText();

			ClassicAssert.AreEqual("1", result);
		}
	}
}