using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla30166 : IssuesUITest
	{
		public Bugzilla30166(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationBar.BarBackgroundColor resets on Lollipop after popping modal page";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue10222Test()
		{
			try
			{
				RunningApp.WaitForElement("PushModal");
				RunningApp.Tap("PushModal");
				RunningApp.WaitForElement("Back");
				RunningApp.Tap("Back");
				RunningApp.Screenshot("Navigation bar should be red");
			}
			finally
			{
				RunningApp.Back();
			}
		}
	}
}