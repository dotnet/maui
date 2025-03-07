#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
//Some tests occasionally fail because cookies are not updated. This issue is difficult to replicate locally and occurs very rarely, even in CI. 
//It causes flakiness, so now ignore this test. Issue Link: https://github.com/dotnet/maui/issues/27854
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.WebView)]
	public class Issue3262 : _IssuesUITest
	{
		public Issue3262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Adding Cookies ability to a WebView...";

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			App.Tap("PageWithoutCookies");
			App.WaitForElement("PageWithoutCookies");
		}

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		[Category(UITestCategories.Compatibility)]
		public void ChangeDuringNavigating()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a couple cookies
			App.WaitForElement("ChangeDuringNavigating");
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
		}

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		[Category(UITestCategories.Compatibility)]
		public void AddAdditionalCookieToWebView()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a couple cookies
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("AdditionalCookie");
			ValidateSuccess();
		}

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		[Category(UITestCategories.Compatibility)]
		public void SetToOneCookie()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			App.Tap("OneCookie");
			ValidateSuccess();
		}

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		[Category(UITestCategories.Compatibility)]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a cookie to verify said cookie remains
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("NullAllCookies");
			ValidateSuccess();
		}

		[Test]
		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27854")]
		[Category(UITestCategories.Compatibility)]
		public void RemoveAllTheCookiesIAdded()
		{
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a cookie so you can remove a cookie
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("EmptyAllCookies");
			ValidateSuccess();
		}

		void ValidateSuccess()
		{
			try
			{
				App.WaitForElement("SuccessCookiesLabel", timeout: TimeSpan.FromSeconds(4));
			}
			catch
			{
				App.Tap("DisplayAllCookies");
				// Tapping "DisplayAllCookies" opens a display alert. Leaving this popup open can cause subsequent test failures.
				// To prevent this, we take a screenshot of the cookies and then tap the "Cancel" button to close the popup before proceeding with further test cases.
				App.Screenshot("Cookies");
				App.Tap("Cancel");
				throw;
			}
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			VerifyInternetConnectivity();
		}
	}
}
#endif