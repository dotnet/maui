using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3275 : _IssuesUITest
	{
		public Issue3275(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue3275Test()
		{
			// Navigate into TransactionsPage via NavigationPage.PushAsync
			App.WaitForElement("btnLeak");
			App.Tap("btnLeak");

			// Scroll to trigger cell recycling (the leak/NRE surface area)
			App.WaitForElement("btnScrollTo");
			App.Tap("btnScrollTo");

			// Navigate back — this triggers OnDisappearing which nulls BindingContext,
			// causing NRE on recycled cells with ContextAction command bindings if bug is present.
			// If the app crashes, the next WaitForElement will fail with app-not-running.
			App.TapBackArrow();

			// Verify we returned to MainPage — OnAppearing sets TestResult to SUCCESS
			App.WaitForElement("TestResult", timeout: TimeSpan.FromSeconds(10));
			var result = App.FindElement("TestResult").GetText();
			Assert.That(result, Is.EqualTo("SUCCESS"), $"Test reported: {result}");
		}
	}
}