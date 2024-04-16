using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Github6384 : IssuesUITest
	{
		public Github6384(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "content page in tabbed page not showing inside shell tab";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("I am at Github6384");
			RunningApp.WaitForElement("NavigationButton");
			RunningApp.Tap("NavigationButton");
			RunningApp.WaitForElement("SubTabLabel1");
			// The label is visible!
			// Note: This check only catches the bug on iOS. Android will pass also without the fix.
			RunningApp.Screenshot("The new page is visible!");
		}
	}
}