#if ANDROID && TEST_FAILS_ON_ANDROID // Related issue: https://github.com/dotnet/maui/issues/26505
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1909 : _IssuesUITest
	{
		public Issue1909(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Xamarin.forms 2.5.0.280555 and android circle button issue";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("https://github.com/dotnet/maui/issues/26505")]
		public void Issue1909Test()
		{
			App.WaitForElement("TestReady");
			VerifyScreenshot();
		}
	}
}
#endif