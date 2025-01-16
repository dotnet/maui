using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1905 : _IssuesUITest
	{
		public Issue1905(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pull to refresh doesn't work if iOS 11 large titles is enabled";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[Category(UITestCategories.Compatibility)]
		public void TestIssue1905RefreshShows()
		{
			App.WaitForElement("btnRefresh");
			VerifyScreenshot();
			App.WaitForElement("data refreshed");
		}
	}
}