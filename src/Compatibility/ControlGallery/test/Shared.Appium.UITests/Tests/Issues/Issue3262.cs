using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
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
			App.Click("PageWithoutCookies");
			App.WaitForElement("PageWithoutCookies");
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void ChangeDuringNavigating()
		{
			App.WaitForElement("Loaded");
			// add a couple cookies
			App.Click("ChangeDuringNavigating");
			ValidateSuccess();
			App.Click("ChangeDuringNavigating");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void AddAdditionalCookieToWebView()
		{
			App.WaitForElement("Loaded");
			// add a couple cookies
			App.Click("AdditionalCookie");
			ValidateSuccess();
			App.Click("AdditionalCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void SetToOneCookie()
		{
			App.WaitForElement("Loaded");
			App.Click("OneCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			App.WaitForElement("Loaded");
			// add a cookie to verify said cookie remains
			App.Click("AdditionalCookie");
			ValidateSuccess();
			App.Click("NullAllCookies");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void RemoveAllTheCookiesIAdded()
		{
			App.WaitForElement("Loaded");
			// add a cookie so you can remove a cookie
			App.Click("AdditionalCookie");
			ValidateSuccess();
			App.Click("EmptyAllCookies");
			ValidateSuccess();
		}

		void ValidateSuccess()
		{
			try
			{
				App.WaitForElement("Success");
			}
			catch
			{
				App.Click("DisplayAllCookies");
				throw;
			}
		}
	}
}