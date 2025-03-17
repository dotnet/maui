#if TEST_FAILS_ON_WINDOWS // AutomationId for Grid is not work on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13476 : _IssuesUITest
	{
		public Issue13476(TestDevice testDevice) : base(testDevice)
		{
		}
#if ANDROID
		const string Page = "PAGE 1";
#else
		const string Page = "page 1";
#endif
		public override string Issue => "Shell Title View Test";
		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void TitleViewHeightDoesntOverflow()
		{
			var titleView = App.WaitForElement("TitleViewId").GetRect();
			var topTab = App.WaitForElement(Page).GetRect();

			var titleViewBottom = titleView.Y + titleView.Height;
			var topTabTop = topTab.Y;

			Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned behind tabs");
		}
	}
}
#endif