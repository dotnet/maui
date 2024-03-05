using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    public class Issue10744 : IssuesUITest
	{
		public Issue10744(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] WebView.Eval crashes on Android with long string\"";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewEvalCrashesOnAndroidWithLongString()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("navigatedLabel");
		}
	}
}
