using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3262 : IssuesUITest
	{
		public Issue3262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Adding Cookies ability to a WebView...";

		[Test]
		[Category(UITestCategories.WebView)]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			RunningApp.Tap("PageWithoutCookies");
			RunningApp.WaitForElement("PageWithoutCookies");
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnIOS]
		public void ChangeDuringNavigating()
		{
			RunningApp.WaitForElement("Loaded");
			// add a couple cookies
			RunningApp.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			RunningApp.Tap("ChangeDuringNavigating");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnIOS]
		public void AddAdditionalCookieToWebView()
		{
			RunningApp.WaitForElement("Loaded");
			// add a couple cookies
			RunningApp.Tap("AdditionalCookie");
			ValidateSuccess();
			RunningApp.Tap("AdditionalCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnIOS]
		public void SetToOneCookie()
		{
			RunningApp.WaitForElement("Loaded");
			RunningApp.Tap("OneCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnIOS]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			RunningApp.WaitForElement("Loaded");
			// add a cookie to verify said cookie remains
			RunningApp.Tap("AdditionalCookie");
			ValidateSuccess();
			RunningApp.Tap("NullAllCookies");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void RemoveAllTheCookiesIAdded()
		{
			RunningApp.WaitForElement("Loaded");
			// add a cookie so you can remove a cookie
			RunningApp.Tap("AdditionalCookie");
			ValidateSuccess();
			RunningApp.Tap("EmptyAllCookies");
			ValidateSuccess();
		}

		void ValidateSuccess()
		{
			try
			{
				RunningApp.WaitForElement("Success");
			}
			catch
			{
				RunningApp.Tap("DisplayAllCookies");
				throw;
			}
		}
	}
}