#if WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue1937 : IssuesUITest
	{
		public Issue1937(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Choppy animation";

		[Test]
		[Category(UITestCategories.Animation)]
		public void Issue1937Test()
		{
			RunningApp.Tap("FAST_TIMER");
			RunningApp.WaitForElement("COMPLETE", timeout: TimeSpan.FromSeconds(2));
			var result = RunningApp.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks1);

			//If fps > 50 then result must be 50. For small fps we use comparing with 35.
			ClassicAssert.IsTrue(timerTicks1 > 35, $"Expected timer ticks are greater than 35. Actual: {timerTicks1}");

			RunningApp.Tap("SLOW_TIMER");
			RunningApp.WaitForElement("COMPLETE", timeout: TimeSpan.FromSeconds(2));
			result = RunningApp.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks2);

			ClassicAssert.IsTrue(timerTicks2 < 11, $"Expected timer ticks are less than 11. Actual: {timerTicks2}");
		}
	}
}
#endif