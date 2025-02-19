#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10744 : _IssuesUITest
	{
		public Issue10744(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] WebView.Eval crashes on Android with long string";

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void WebViewEvalCrashesOnAndroidWithLongString()
		{
			App.WaitForElement("navigatedLabel");
		}
	}
}
#endif