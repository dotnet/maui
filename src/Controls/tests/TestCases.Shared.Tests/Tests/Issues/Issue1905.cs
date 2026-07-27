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
		[Category(UITestCategories.ListView)]
		public void TestIssue1905RefreshShows()
		{
			// Self-verifying: tap Run Test, the app calls BeginRefresh() on a ListView
			// and verifies the RefreshCommand executes. Original bug: refresh doesn't
			// trigger when large titles are enabled.
			App.WaitForElement("RunTest");
			App.Tap("RunTest");

			// Wait for the refresh cycle to complete, then check the result
			Task.Delay(5000).Wait();
			var result = App.FindElement("TestResult").GetText();
			Assert.That(result, Is.EqualTo("SUCCESS"), $"Test reported: {result}");
		}
	}
}