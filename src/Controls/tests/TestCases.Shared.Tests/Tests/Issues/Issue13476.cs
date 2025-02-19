/*
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13476 : _IssuesUITest
	{
		public Issue13476(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shell Title View Test";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void TitleViewHeightDoesntOverflow()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			var titleView = App.WaitForElement("TitleViewId").GetRect();
			var topTab = App.WaitForElement("page 1").GetRect();

			var titleViewBottom = titleView.Y + titleView.Height;
			var topTabTop = topTab.Y;

			ClassicAssert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned behind tabs");
		}
	}
}
*/