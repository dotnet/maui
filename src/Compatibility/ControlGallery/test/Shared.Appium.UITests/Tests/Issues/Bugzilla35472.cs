using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla35472 : IssuesUITest
	{
		public Bugzilla35472(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync during ScrollToAsync throws NullReferenceException";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue35472PopAsyncDuringAnimatedScrollToAsync()
		{
			try
			{
				RunningApp.WaitForElement("PushButton");
				RunningApp.Tap("PushButton");

				RunningApp.WaitForElement("NowPushButton");
				RunningApp.Screenshot("On Page With ScrollView");
				RunningApp.Tap("NowPushButton");

				RunningApp.WaitForNoElement("The test has passed");
				RunningApp.Screenshot("Success");
			}
			finally
			{
				RunningApp.Back();
			}
		}
	}
}