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

			// Wait for navigation to complete (network navigation can be slow)
			App.WaitForTextToBePresentInElement("StatusLabel", "NavigationComplete", timeout: TimeSpan.FromSeconds(30));

			var status = App.FindElement("StatusLabel").GetText();
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

			// Wait for the blocked detection (timer-based, ~3 seconds + buffer)
			App.WaitForTextToBePresentInElement("BlockedLabel", "NavigationBlocked", timeout: TimeSpan.FromSeconds(15));

			var blockedText = App.FindElement("BlockedLabel").GetText();
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
