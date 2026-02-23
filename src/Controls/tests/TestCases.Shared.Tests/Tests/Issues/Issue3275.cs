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
			// Self-verifying: tap Run Test, the app pushes a modal ListView page with
			// RecycleElement + ContextActions, performs ScrollTo, pops (triggering
			// OnDisappearing which nulls BindingContext), and reports result.
			// If the original bug is present, the app crashes with NRE.
			App.WaitForElement("RunTest");
			App.Tap("RunTest");

			// Wait for the async navigation flow (push, scroll, pop) to complete
			Task.Delay(5000).Wait();
			var result = App.FindElement("TestResult").GetText();
			Assert.That(result, Is.EqualTo("SUCCESS"), $"Test reported: {result}");
		}
	}
}