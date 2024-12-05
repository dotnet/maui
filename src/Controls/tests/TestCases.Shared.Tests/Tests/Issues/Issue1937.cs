#if WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue1937Test()
		{
			App.WaitForElement("FAST_TIMER");
			App.Tap("FAST_TIMER");
			App.WaitForNoElement("COMPLETE", timeout: TimeSpan.FromSeconds(2));
			var result = App.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks1);

			//If fps > 50 then result must be 50. For small fps we use comparing with 35.
			ClassicAssert.IsTrue(timerTicks1 > 35, $"Expected timer ticks are greater than 35. Actual: {timerTicks1}");

			App.Tap("SLOW_TIMER");
			App.WaitForNoElement("COMPLETE", timeout: TimeSpan.FromSeconds(2));
			result = App.WaitForElement("RESULT");
			int.TryParse(result.GetText(), out int timerTicks2);

			ClassicAssert.IsTrue(timerTicks2 < 11, $"Expected timer ticks are less than 11. Actual: {timerTicks2}");
		}
	}
}
#endif