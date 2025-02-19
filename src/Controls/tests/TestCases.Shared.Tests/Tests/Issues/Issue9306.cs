#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9306 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue9306SwipeViewCloseSwiping()
		{
			App.WaitForElement(SwipeViewId);

			App.SwipeLeftToRight(SwipeViewId);
			App.SwipeRightToLeft(SwipeViewId);
			App.SwipeLeftToRight(SwipeViewId);

			App.Tap(SwipeItemId);

			var result = App.FindElement(LeftCountLabelId).GetText();

			ClassicAssert.AreEqual("1", result);
		}
	}
}
#endif