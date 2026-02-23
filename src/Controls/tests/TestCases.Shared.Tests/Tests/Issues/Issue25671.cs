#if ANDROID || IOS //This test is only valid for Android and iOS as it measures the layout passes.
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25671 : _IssuesUITest
	{
		public Issue25671(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Layout passes should not increase";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public async Task LayoutPassesShouldNotIncrease()
		{
			App.WaitForElement("RegenerateItems");

			await ScrollDown();
			await ScrollDown();
			await ScrollDown();
			await ScrollUp();
			await ScrollUp();

			App.Tap("RegenerateItems");

			await ScrollDown();
			await ScrollDown();
			await ScrollUp();
			await ScrollUp();

			App.Tap("PressMe");
			// Text will be in the format "M: 0, A: 0"
			var text = App.WaitForElement("PressMe").GetText()!;
			// Get measure passes and arrange passes value via regex
			var match = System.Text.RegularExpressions.Regex.Match(text, @"M: (\d+), A: (\d+)");
			var measurePasses = int.Parse(match.Groups[1].Value);
			var arrangePasses = int.Parse(match.Groups[2].Value);

#if IOS
			var maxMeasurePasses = 203;
			var maxArrangePasses = 214;

            if (App.FindElement("HeadingLabel").GetText() == "CollectionViewHandler2")
            {
	            maxMeasurePasses = 373;
	            maxArrangePasses = 276;
            }
#elif ANDROID
			const int maxMeasurePasses = 353;
			const int maxArrangePasses = 337;
#endif

			var logMessage = @$"Measure passes: {measurePasses}, Arrange passes: {arrangePasses}";
			TestContext.WriteLine(logMessage);

			// Write the log to a file and attach it to the test results for ADO
			var logFile = Path.Combine(Path.GetTempPath(), "LayoutPasses.log");
			File.WriteAllText(logFile, logMessage);
			TestContext.AddTestAttachment(logFile, "LayoutPasses.log");

			// Then assert that the measure passes and arrange passes are less than the expected values.
			// Let's give a 5% margin of error.
			ClassicAssert.LessOrEqual(measurePasses, maxMeasurePasses * 1.05);
			ClassicAssert.LessOrEqual(arrangePasses, maxArrangePasses * 1.05);
		}

		async Task ScrollDown()
		{
			await Task.Delay(50);
			App.ScrollDown("CV", swipePercentage: 0.5D, swipeSpeed: 1000);
			await Task.Delay(50);
		}

		async Task ScrollUp()
		{
			await Task.Delay(50);
			App.ScrollUp("CV", swipePercentage: 0.5D, swipeSpeed: 1000);
			await Task.Delay(50);
		}
	}
}
#endif