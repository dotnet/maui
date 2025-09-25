# if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS    // BackButtonTitle is only applicable for iOS and macOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31539 : _IssuesUITest
	{
		public Issue31539(TestDevice device) : base(device) { }

		public override string Issue => "[iOS, macOS] Navigation Page BackButtonTitle Not Updating";
		[Test]
		[Category(UITestCategories.Navigation)]
		public void VerifyBackButtonTitleUpdates()
		{
			App.WaitForElement("PushSecondPage");
			App.Tap("PushSecondPage");
			App.TapBackArrow();
			App.Tap("SetBackButtonTitle");
			App.Tap("PushSecondPage");
			App.WaitForElement("SecondPageLabel");
			VerifyScreenshot();
		}
	}
}
#endif