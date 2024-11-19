using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3262 : _IssuesUITest
	{
		public Issue3262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Adding Cookies ability to a WebView...";

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			App.Tap("PageWithoutCookies");
			App.WaitForElement("PageWithoutCookies");
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void ChangeDuringNavigating()
		{
			App.WaitForElement("Loaded");
			// add a couple cookies
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
			App.Tap("ChangeDuringNavigating");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void AddAdditionalCookieToWebView()
		{
			App.WaitForElement("Loaded");
			// add a couple cookies
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("AdditionalCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void SetToOneCookie()
		{
			App.WaitForElement("Loaded");
			App.Tap("OneCookie");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			App.WaitForElement("Loaded");
			// add a cookie to verify said cookie remains
			App.Tap("AdditionalCookie");
			ValidateSuccess();
			App.Tap("NullAllCookies");
			ValidateSuccess();
		}

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void RemoveAllTheCookiesIAdded()
		{
			App.WaitForElement("Loaded");
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
				App.WaitForElement("Success");
			}
			catch
			{
				App.Tap("DisplayAllCookies");
				throw;
			}
		}
	}
}