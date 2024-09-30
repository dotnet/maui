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

		
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			try
			{
				App.Tap("PageWithoutCookies");
				App.WaitForElement("PageWithoutCookies");
			}
			catch
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void ChangeDuringNavigating()
		{
			try
			{
				App.WaitForElement("SuccessNavigationLabel");
				// add a couple cookies
				App.Tap("ChangeDuringNavigating");
				ValidateSuccess();
				App.Tap("ChangeDuringNavigating");
				ValidateSuccess();
			}
			catch
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void AddAdditionalCookieToWebView()
		{
			try
			{
				App.WaitForElement("SuccessNavigationLabel");
				// add a couple cookies
				App.Tap("AdditionalCookie");
				ValidateSuccess();
				App.Tap("AdditionalCookie");
				ValidateSuccess();
			}
			catch
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void SetToOneCookie()
		{
			try
			{
				App.WaitForElement("SuccessNavigationLabel");
				App.Tap("OneCookie");
				ValidateSuccess();
			}
			catch
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			try
			{
				App.WaitForElement("SuccessNavigationLabel");
				// add a cookie to verify said cookie remains
				App.Tap("AdditionalCookie");
				ValidateSuccess();
				App.Tap("NullAllCookies");
				ValidateSuccess();
			}
			catch
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void RemoveAllTheCookiesIAdded()
		{
			try
			{
				App.WaitForElement("SuccessNavigationLabel");
				// add a cookie so you can remove a cookie
				App.Tap("AdditionalCookie");
				ValidateSuccess();
				App.Tap("EmptyAllCookies");
				ValidateSuccess();
			}
			catch
			{
				Assert.Fail();
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
	}
}