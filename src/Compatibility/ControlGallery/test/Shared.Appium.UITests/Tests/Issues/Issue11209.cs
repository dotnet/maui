using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    public class Issue11209 : IssuesUITest
	{
		const string SwipeViewContent = "SwipeViewContent";
		const string Success = "Success";

		public Issue11209(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS][SwipeView] Swipe view not handling tap gesture events until swiped";

		[Test]
		public void TapSwipeViewAndNavigateTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(SwipeViewContent);
			App.Click(SwipeViewContent);
			App.WaitForElement(Success);
		}
	}
}