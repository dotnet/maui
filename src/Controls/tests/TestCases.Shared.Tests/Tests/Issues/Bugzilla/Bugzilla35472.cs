using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla35472 : _IssuesUITest
	{
		public Bugzilla35472(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync during ScrollToAsync throws NullReferenceException";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue35472PopAsyncDuringAnimatedScrollToAsync()
		{
			try
			{
				App.WaitForElement("PushButton");
				App.Tap("PushButton");

				App.WaitForElement("NowPushButton");
				App.Tap("NowPushButton");

				App.WaitForElement("The test has passed");
			}
			finally
			{
				App.Back();
			}
		}
	}
}