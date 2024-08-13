#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue23267 : _IssuesUITest
	{
		public Issue23267(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CornerRadius of GradientDrawable doesn't work anymore for ImageButton";

		[Test]
		[Category(UITestCategories.ImageButton)]
		[FailsOnAndroid]
		public void WebViewEvalCrashesOnAndroidWithLongString()
		{
			App.WaitForElement("ImageButtonId");
			App.Tap("ImageButtonId");
			VerifyScreenshot();
		}
	}
}
#endif