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
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			App.Tap("PageWithoutCookies");
			App.WaitForElement("PageWithoutCookies");
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void ChangeDuringNavigating()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a couple cookies
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void AddAdditionalCookieToWebView()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a couple cookies
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void SetToOneCookie()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			App.Tap("OneCookie");
			ValidateSuccess();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a cookie to verify said cookie remains
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("NullAllCookies");
			ValidateSuccess();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void RemoveAllTheCookiesIAdded()
		{
			if (IsInternetConnectionAvailable())
            {
			App.WaitForElement("SuccessfullPageLoadLabel");
			// add a cookie so you can remove a cookie
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("EmptyAllCookies");
			ValidateSuccess();
			}
		}

		void ValidateSuccess()
		{
			try
			{
				App.WaitForElement("SuccessCookiesLabel");
			}
			catch
			{
				App.Tap("DisplayAllCookies");
				throw;
			}
		}
		 private bool IsInternetConnectionAvailable()
        {
            var internetConnectionLabel = App.FindElements("Internet Connection Available");
            return internetConnectionLabel.Count > 0;
        }
	}
}