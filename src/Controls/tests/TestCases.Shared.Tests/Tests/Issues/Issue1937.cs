using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1937 : _IssuesUITest
	{
		public Issue1937(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Choppy animation";

		[Test]
		[Category(UITestCategories.Animation)]
		[Category(UITestCategories.Compatibility)]
		public void Issue1937Test()
		{
			App.WaitForElement("FAST_TIMER");
			App.Tap("FAST_TIMER");
			Thread.Sleep(TimeSpan.FromSeconds(1));
			App.WaitForElement("COMPLETE");
			var result = App.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks1);

			//If fps > 50 then result must be 50. For small fps we use comparing with 30.
			Assert.That(timerTicks1, Is.GreaterThan(30),
				$"Expected timer ticks are greater than 30. Actual: {timerTicks1}");

			App.Tap("SLOW_TIMER");
			Thread.Sleep(TimeSpan.FromSeconds(1));
			App.WaitForElement("COMPLETE");
			result = App.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks2);

			Assert.That(timerTicks2, Is.LessThan(11),
				$"Expected timer ticks are less than 11. Actual: {timerTicks2}");
		}
	}
}
