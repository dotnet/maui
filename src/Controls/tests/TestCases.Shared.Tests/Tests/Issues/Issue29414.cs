using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29414 : _IssuesUITest
	{
		public Issue29414(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Loaded event not triggered when navigating back to a previous page";

		[Test]
		[Category(UITestCategories.Page)]
		[Category(UITestCategories.Navigation)]
		public void LoadedEventShouldFireWhenNavigatingBackToPage()
		{
			// Step 1: Verify the main page loaded event fired initially
			App.WaitForElement("MainPageLoadedCount");
			var initialLoadedText = App.FindElement("MainPageLoadedCount").GetText();
			Assert.That(initialLoadedText, Is.EqualTo("Main Page Loaded Count: 1"), 
				"Main page should have loaded count of 1 initially");

			// Step 2: Navigate to second page
			App.WaitForElement("NavigateToSecondPageButton");
			App.Tap("NavigateToSecondPageButton");

			// Step 3: Verify second page loads
			App.WaitForElement("SecondPageLoadedCount");
			var secondPageLoadedText = App.FindElement("SecondPageLoadedCount").GetText();
			Assert.That(secondPageLoadedText, Is.EqualTo("Second Page Loaded Count: 1"), 
				"Second page should have loaded count of 1");

			// Step 4: Navigate back to main page
			App.WaitForElement("NavigateBackToMainPageButton");
			App.Tap("NavigateBackToMainPageButton");

			// Step 5: Verify main page loaded event fired again (this is the failing test case)
			App.WaitForElement("MainPageLoadedCount");
			var finalLoadedText = App.FindElement("MainPageLoadedCount").GetText();
			Assert.That(finalLoadedText, Is.EqualTo("Main Page Loaded Count: 2"), 
				"Main page should have loaded count of 2 after navigating back - THIS IS THE BUG ON ANDROID");
		}

		[Test]
		[Category(UITestCategories.Page)]
		[Category(UITestCategories.Navigation)]
		public void LoadedEventShouldFireMultipleTimesWhenNavigatingBackAndForth()
		{
			// Navigate back and forth multiple times to ensure loaded events keep firing
			App.WaitForElement("MainPageLoadedCount");
			
			// Initial state
			var mainPageText = App.FindElement("MainPageLoadedCount").GetText();
			Assert.That(mainPageText, Is.EqualTo("Main Page Loaded Count: 1"));

			// Navigate to second page and back - first time
			App.Tap("NavigateToSecondPageButton");
			App.WaitForElement("NavigateBackToMainPageButton");
			App.Tap("NavigateBackToMainPageButton");
			
			App.WaitForElement("MainPageLoadedCount");
			mainPageText = App.FindElement("MainPageLoadedCount").GetText();
			Assert.That(mainPageText, Is.EqualTo("Main Page Loaded Count: 2"), 
				"Main page should have loaded count of 2 after first back navigation");

			// Navigate to second page and back - second time
			App.Tap("NavigateToSecondPageButton");
			App.WaitForElement("NavigateBackToMainPageButton");
			App.Tap("NavigateBackToMainPageButton");
			
			App.WaitForElement("MainPageLoadedCount");
			mainPageText = App.FindElement("MainPageLoadedCount").GetText();
			Assert.That(mainPageText, Is.EqualTo("Main Page Loaded Count: 3"), 
				"Main page should have loaded count of 3 after second back navigation");
		}
	}
}