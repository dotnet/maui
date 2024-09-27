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

		protected override bool ResetAfterEachTest => true;
		public override string Issue => "Adding Cookies ability to a WebView...";

		[Test]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			App.Tap("PageWithoutCookies");
			App.WaitForElement("PageWithoutCookies");
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void ChangeDuringNavigating()
		{
			App.WaitForElement("SuccessNavigationLabel");
			// add a couple cookies
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void AddAdditionalCookieToWebView()
		{
			App.WaitForElement("SuccessNavigationLabel");
			// add a couple cookies
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("AdditionalCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void SetToOneCookie()
		{
			App.WaitForElement("SuccessNavigationLabel");
			App.Tap("OneCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			App.WaitForElement("SuccessNavigationLabel");
			// add a cookie to verify said cookie remains
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("NullAllCookies");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void RemoveAllTheCookiesIAdded()
		{
			App.WaitForElement("SuccessNavigationLabel");
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
				App.WaitForElement("SuccessCookiesLabel");
			}
			catch
			{
				App.Tap("DisplayAllCookies");
				throw;
			}
		}
	}
}