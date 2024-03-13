using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue13476 : IssuesUITest
	{
		public Issue13476(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shell Title View Test";

		[Test]
		[Category(UITestCategories.Shell)]
		public void TitleViewHeightDoesntOverflow()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			var titleView = RunningApp.WaitForElement("TitleViewId").GetRect();
			var topTab = RunningApp.WaitForElement("page 1").GetRect();

			var titleViewBottom = titleView.Y + titleView.Height;
			var topTabTop = topTab.Y;

			ClassicAssert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned behind tabs");
		}
	}
}