using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue35470 : _IssuesUITest
	{
		public Issue35470(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "WebView AllowedDomains blocks navigation to non-allowed domains";

		[Test]
		[Category(UITestCategories.WebView)]
		public void AllowedDomainNavigationSucceeds()
		{
			VerifyInternetConnectivity();

			App.WaitForElement("LoadAllowedButton");
			App.Tap("LoadAllowedButton");

			// Wait for navigation to complete
			App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(15));
			var status = App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(15)).GetText();

			// The navigation should complete successfully (either NavigationComplete or the page loads)
			// Give it some time since network navigation can be slow
			for (int i = 0; i < 10 && status == "Loading..."; i++)
			{
				Thread.Sleep(1000);
				status = App.FindElement("StatusLabel").GetText();
			}

			Assert.That(status, Is.EqualTo("NavigationComplete"),
				"Navigation to an allowed domain should succeed");
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void BlockedDomainNavigationIsBlocked()
		{
			VerifyInternetConnectivity();

			App.WaitForElement("LoadBlockedButton");
			App.Tap("LoadBlockedButton");

			// Wait for the blocked detection timer (3 seconds + buffer)
			App.WaitForElement("BlockedLabel", timeout: TimeSpan.FromSeconds(10));

			// Poll for the "NavigationBlocked" text
			string blockedText = "";
			for (int i = 0; i < 10; i++)
			{
				blockedText = App.FindElement("BlockedLabel").GetText();
				if (blockedText == "NavigationBlocked")
					break;
				Thread.Sleep(1000);
			}

			Assert.That(blockedText, Is.EqualTo("NavigationBlocked"),
				"Navigation to a blocked domain should be blocked");
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void RemovingAllowedDomainsAllowsAll()
		{
			App.WaitForElement("RemoveAllowedDomainsButton");
			App.Tap("RemoveAllowedDomainsButton");

			var status = App.FindElement("StatusLabel").GetText();
			Assert.That(status, Is.EqualTo("AllowedDomainsRemoved"),
				"AllowedDomains should be removable at runtime");
		}
	}
}
